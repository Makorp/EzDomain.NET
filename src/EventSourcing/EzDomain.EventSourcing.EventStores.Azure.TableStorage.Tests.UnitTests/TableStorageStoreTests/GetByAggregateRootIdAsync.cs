using System.Linq.Expressions;
using EzDomain.EventSourcing.EventStores.Azure.TableStorage.Tests.UnitTests.TestDoubles;
using EzDomain.EventSourcing.Serialization;
using Microsoft.Extensions.Logging;

namespace EzDomain.EventSourcing.EventStores.Azure.TableStorage.Tests.UnitTests.TableStorageStoreTests;

[TestFixture]
public sealed class GetByAggregateRootIdAsync
{
    private const string ExistingAggregateRootId = "ExistingAggregateRootId";

    private readonly IReadOnlyList<TableEntity> _tableEntities = new[]
    {
        new TableEntity(ExistingAggregateRootId, "0"),
        new TableEntity(ExistingAggregateRootId, "1")
    };

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
            .Setup(m => m.QueryAsync(
                It.IsAny<Expression<Func<TableEntity, bool>>>(),
                null,
                null,
                It.IsAny<CancellationToken>()))
            .Returns((Expression<Func<TableEntity, bool>> expression, int? _, IEnumerable<string> _, CancellationToken _) =>
            {
                var isFilterTrue = true;
                foreach (var tableEntity in _tableEntities)
                {
                    if (!expression.Compile().Invoke(tableEntity))
                    {
                        isFilterTrue = false;
                    }
                }

                var tableEntities = isFilterTrue
                    ? _tableEntities
                    : Array.Empty<TableEntity>();

                return AsyncPageable<TableEntity>.FromPages(new[]
                {
                    Page<TableEntity>.FromValues(tableEntities, null, Mock.Of<Response>())
                });
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
    public async Task GetByAggregateRootIdAsync_ReturnsAggregateRootEvents_WhenAggregateRootIdAndFromVersionAreProvided()
    {
        // Arrange
        var eventStore = new TableStorageStore(
            _mockLogger.Object,
            _mockEventDataSerializer.Object,
            _mockTableServiceClient.Object);

        // Act
        var aggregateRootEvents = await eventStore.GetByAggregateRootIdAsync(
            ExistingAggregateRootId,
            Constants.InitialVersion,
            CancellationToken.None);

        // Assert
        aggregateRootEvents
            .Should()
            .HaveCount(_tableEntities.Count);

        _mockTableServiceClient.Verify(m => m.GetTableClient(It.IsAny<string>()), Times.Once);
        _mockTableClient.Verify(m =>
                m.QueryAsync(
                    It.IsAny<Expression<Func<TableEntity, bool>>>(),
                    null,
                    null,
                    It.IsAny<CancellationToken>()),
            Times.Once);

        _mockEventDataSerializer.Verify(m => m.Deserialize(It.IsAny<string>(), It.IsAny<string>()), Times.Exactly(_tableEntities.Count));
    }

    [Test]
    public async Task GetByAggregateRootIdAsync_ReturnsEmptyDomainEventStream_WhenAggregateRootIdIsInvalid()
    {
        // Arrange
        const short expectedNumberOfDomainEvents = 0;

        var eventStore = new TableStorageStore(
            _mockLogger.Object,
            _mockEventDataSerializer.Object,
            _mockTableServiceClient.Object);

        // Act
        var aggregateRootEvents = await eventStore.GetByAggregateRootIdAsync(
            Guid.Empty.ToString(),
            Constants.InitialVersion,
            CancellationToken.None);

        // Assert
        aggregateRootEvents
            .Should()
            .HaveCount(expectedNumberOfDomainEvents);

        _mockTableServiceClient.Verify(m => m.GetTableClient(It.IsAny<string>()), Times.Once);
        _mockTableClient.Verify(m =>
                m.QueryAsync(
                    It.IsAny<Expression<Func<TableEntity, bool>>>(),
                    null,
                    null,
                    It.IsAny<CancellationToken>()),
            Times.Once);

        _mockEventDataSerializer.Verify(m => m.Deserialize(It.IsAny<string>(), It.IsAny<string>()), Times.Exactly(expectedNumberOfDomainEvents));
    }
}