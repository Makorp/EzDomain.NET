using EzDomain.EventSourcing.Domain.Model;
using EzDomain.EventSourcing.EventStores.Azure.TableStorage.Tests.UnitTests.TestDoubles;
using EzDomain.EventSourcing.Exceptions;
using EzDomain.EventSourcing.Serialization;
using Microsoft.Extensions.Logging;

namespace EzDomain.EventSourcing.EventStores.Azure.TableStorage.Tests.UnitTests.TableStorageStoreTests;

[TestFixture]
public class SaveAsyncTests
{
    private const string ExistingAggregateRootId = "ExistingAggregateRootId";
    private const long ExistingAggregateRootVersion = 0;

    private readonly Mock<ILogger> _mockLogger = new();
    private readonly Mock<IEventDataSerializer<string>> _mockEventDataSerializer = new();
    private readonly Mock<TableServiceClient> _mockTableServiceClient = new();
    private readonly Mock<TableClient> _mockTableClient = new();

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _mockTableServiceClient
            .Setup(m => m.GetTableClient(It.IsAny<string>()))
            .Returns(_mockTableClient.Object);

        _mockTableClient
            .Setup(m => m.SubmitTransactionAsync(It.IsAny<IEnumerable<TableTransactionAction>>(), It.IsAny<CancellationToken>()))
            .Callback((IEnumerable<TableTransactionAction> tableTransactionActions, CancellationToken _) =>
            {
                if (tableTransactionActions.Any(tta => long.Parse(tta.Entity.RowKey) <= ExistingAggregateRootVersion))
                {
                    throw new TableTransactionFailedException(
                        new RequestFailedException(
                            409,
                            nameof(RequestFailedException),
                            "EntityAlreadyExists",
                            new Exception()));
                }
            });

        _mockEventDataSerializer
            .Setup(m => m.Deserialize(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(new FakeEvent());
    }

    [TearDown]
    public void TearDown()
    {
        _mockTableServiceClient.Invocations.Clear();
        _mockTableClient.Invocations.Clear();
        _mockEventDataSerializer.Invocations.Clear();
    }

    [Test]
    public async Task SaveAsync_DoesNotThrowConcurrencyException_WhenProvidedEventsAreNotConcurrent()
    {
        // Arrange
        const int numberOfDomainEvents = 2;

        // Act
        await ExecuteTestAsync(
            numberOfDomainEvents,
            eventStream =>
            {
                var existingAggregateRootVersion = ExistingAggregateRootVersion;

                for (var i = 0; i < eventStream.Length; i++)
                {
                    var fakeEvent = new FakeEvent(ExistingAggregateRootId);
                    fakeEvent.IncrementVersion(ref existingAggregateRootVersion);

                    eventStream[i] = fakeEvent;
                }
            },
            async act =>
            {
                await act
                    .Should()
                    .NotThrowAsync();
            });
    }

    [Test]
    public async Task SaveAsync_ThrowsConcurrencyException_WhenProvidedEventsAreConcurrent()
    {
        // Arrange
        const int numberOfDomainEvents = 2;

        // Act
        await ExecuteTestAsync(
            numberOfDomainEvents,
            eventStream =>
            {
                for (var i = 0; i < eventStream.Length; i++)
                {
                    eventStream[i] = new FakeEvent(ExistingAggregateRootId);
                }
            },
            async act =>
            {
                await act
                    .Should()
                    .ThrowAsync<ConcurrencyException>();
            });
    }

    private async Task ExecuteTestAsync(int numberOfDomainEvents, Action<DomainEvent[]> addDomainEvents, Func<Func<Task>, Task> assert)
    {
        // Arrange
        var eventStore = new TableStorageStore(
            _mockLogger.Object,
            _mockEventDataSerializer.Object,
            _mockTableServiceClient.Object);

        var eventStream = new DomainEvent[numberOfDomainEvents];

        addDomainEvents(eventStream);

        // Act
        var act = async () => await eventStore.AppendToStreamAsync(eventStream, CancellationToken.None);

        // Assert
        await assert(act);

        _mockTableServiceClient.Verify(m => m.GetTableClient(It.IsAny<string>()), Times.Once);
        _mockTableClient.Verify(m => m.SubmitTransactionAsync(It.IsAny<IEnumerable<TableTransactionAction>>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockEventDataSerializer.Verify(m => m.Serialize(It.IsAny<DomainEvent>()), Times.Exactly(numberOfDomainEvents));
    }
}