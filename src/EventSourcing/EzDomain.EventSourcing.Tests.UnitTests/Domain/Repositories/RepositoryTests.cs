using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EzDomain.EventSourcing.Domain.EventStores;
using EzDomain.EventSourcing.Domain.Factories;
using EzDomain.EventSourcing.Domain.Model;
using EzDomain.EventSourcing.Domain.Repositories;
using EzDomain.EventSourcing.Exceptions;
using EzDomain.EventSourcing.Tests.UnitTests.TestDoubles;
using Moq;

namespace EzDomain.EventSourcing.Tests.UnitTests.Domain.Repositories;

[TestFixture]
public sealed class RepositoryTests
{
    private readonly string _aggregateRootIdValue = Guid.NewGuid().ToString();

    private readonly ICollection<DomainEvent> _domainEvents = new List<DomainEvent>();

    private readonly Mock<ITestAggregateRoot> _mockAggregateRoot = new();
    private readonly Mock<IAggregateRootFactory<ITestAggregateRoot, TestAggregateRootId>> _mockFactory = new();
    private readonly Mock<IEventStore> _mockEventStore = new();

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _mockAggregateRoot.As<IAggregateRootBehavior>();

        _mockAggregateRoot
            .SetupGet(m => m.Id)
            .Returns(new TestAggregateRootId(_aggregateRootIdValue));

        _mockFactory
            .Setup(m => m.Create())
            .Returns(() => _mockAggregateRoot.Object);

        _mockEventStore
            .Setup(m => m.GetByAggregateRootIdAsync(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string aggregateRootId, long fromVersion, CancellationToken _) =>
                _domainEvents
                    .Where(domainEvent => domainEvent.AggregateRootId == aggregateRootId && domainEvent.Version > fromVersion)
                    .OrderBy(domainEvent => domainEvent.Version)
                    .ToList());

        _mockEventStore
            .Setup(m => m.SaveAsync(It.IsAny<IReadOnlyCollection<DomainEvent>>(), It.IsAny<CancellationToken>()))
            .Returns((IReadOnlyCollection<DomainEvent> events, CancellationToken _) =>
            {
                foreach (var domainEvent in events)
                {
                    _domainEvents.Add(domainEvent);
                }

                return Task.FromResult(0);
            });
    }

    [SetUp]
    public void SetUp()
    {
        var version = Constants.InitialVersion;

        _domainEvents.Add(new BehaviorExecuted(_aggregateRootIdValue));

        foreach (var domainEvent in _domainEvents)
        {
            domainEvent.IncrementVersion(ref version);
        }
    }

    [TearDown]
    public void TearDown()
    {
        _domainEvents.Clear();

        _mockEventStore.Invocations.Clear();
    }

    [Test]
    public async Task GetByIdAsync_ReturnsAggregateRootInItsCorrectState_WhenAggregateRootIdIsGiven()
    {
        // Arrange
        var repository = new Repository<ITestAggregateRoot, TestAggregateRootId>(_mockFactory.Object, _mockEventStore.Object);

        // Act
        await repository.GetByIdAsync(_aggregateRootIdValue);

        // Assert
        _mockEventStore.Verify(m => m.GetByAggregateRootIdAsync(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<CancellationToken>()), Times.Once());
        _mockFactory.Verify(m => m.Create(), Times.Once());
        _mockAggregateRoot.As<IAggregateRootBehavior>().Verify(m => m.RestoreFromStream(It.IsAny<IReadOnlyCollection<DomainEvent>>()), Times.Once());
    }

    [Test]
    public async Task GetByIdAsync_ReturnsNull_WhenAggregateRootIdIsInvalid()
    {
        // Arrange
        var repository = new Repository<ITestAggregateRoot, TestAggregateRootId>(_mockFactory.Object, _mockEventStore.Object);

        // Act
        var aggregateRoot = await repository.GetByIdAsync(Guid.Empty.ToString());

        // Assert
        aggregateRoot
            .Should()
            .BeNull();

        _mockEventStore.Verify(m => m.GetByAggregateRootIdAsync(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<CancellationToken>()), Times.Once());
    }

    [Test]
    public async Task GIVEN_no_aggregate_root_WHEN_attempting_to_save_THEN_throws_AggregateRootNullException()
    {
        // Arrange
        var repository = new Repository<ITestAggregateRoot, TestAggregateRootId>(_mockFactory.Object, _mockEventStore.Object);

        // Act
        Func<Task> act = async () => await repository.SaveAsync(null);

        // Assert
        await act
            .Should()
            .ThrowAsync<AggregateRootNullException>();

        _mockEventStore.Verify(m => m.SaveAsync(It.IsAny<IReadOnlyCollection<DomainEvent>>(), It.IsAny<CancellationToken>()), Times.Never());
    }

    [Test]
    public async Task GIVEN_unchanged_aggregate_root_WHEN_attempting_to_save_THEN_returns_no_new_changes()
    {
        // Arrange
        var repository = new Repository<ITestAggregateRoot, TestAggregateRootId>(_mockFactory.Object, _mockEventStore.Object);

        var aggregateRoot = new TestAggregateRoot();

        ((IAggregateRootBehavior)aggregateRoot).RestoreFromStream(_domainEvents.ToList());

        // Act
        var aggregateRootChanges = await repository.SaveAsync(aggregateRoot);

        // Assert
        _mockEventStore.Verify(m => m.SaveAsync(It.IsAny<IReadOnlyCollection<DomainEvent>>(), It.IsAny<CancellationToken>()), Times.Never());

        aggregateRootChanges.Count
            .Should()
            .Be(0);
    }

    [Test]
    [AutoData]
    public async Task GIVEN_changed_aggregate_root_WHEN_attempting_to_save_THEN_saves_aggregate_root_new_events(string serializedAggregateRootId)
    {
        // Arrange
        var repository = new Repository<ITestAggregateRoot, TestAggregateRootId>(_mockFactory.Object, _mockEventStore.Object);

        var aggregateRootId = new TestAggregateRootId(serializedAggregateRootId);
        var aggregateRoot = new TestAggregateRoot(aggregateRootId);

        aggregateRoot.ExecuteBehavior();

        // Act
        var aggregateRootChanges = await repository.SaveAsync(aggregateRoot);

        // Assert
        _mockEventStore.Verify(m => m.SaveAsync(It.IsAny<IReadOnlyCollection<DomainEvent>>(), It.IsAny<CancellationToken>()), Times.Once());

        aggregateRootChanges.Count
            .Should()
            .BeGreaterThan(0);
    }
}