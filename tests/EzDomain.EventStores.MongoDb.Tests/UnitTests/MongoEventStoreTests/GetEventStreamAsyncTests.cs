using EzDomain.EventStores.MongoDb.Tests.TestDoubles;

namespace EzDomain.EventStores.MongoDb.Tests.UnitTests.MongoEventStoreTests;

[TestFixture]
internal sealed class GetEventStreamAsyncTests
{
    private readonly List<MongoEventStore.DomainEventSchema> _domainEvents = new();

    private int _moveNextAsyncCallCount;

    private readonly Mock<ILogger> _mockLogger = new();
    private readonly Mock<IMongoClient> _mockMongoClient = new();
    private readonly Mock<IMongoDatabase> _mockMongoDatabase = new();
    private readonly Mock<IMongoCollection<MongoEventStore.DomainEventSchema>> _mockMongoCollection = new();
    private readonly Mock<IAsyncCursor<MongoEventStore.DomainEventSchema>> _mockAsyncCursor = new();

    private readonly MongoEventStoreSettings _mongoEventStoreSettings = new(
        "dummyEventStore",
        "domainEvents");

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _mockMongoClient
            .Setup(m => m.GetDatabase(
                It.IsAny<string>(),
                It.IsAny<MongoDatabaseSettings>()))
            .Returns(_mockMongoDatabase.Object);

        _mockMongoDatabase
            .Setup(m => m.GetCollection<MongoEventStore.DomainEventSchema>(
                It.IsAny<string>(),
                It.IsAny<MongoCollectionSettings>()))
            .Returns(_mockMongoCollection.Object);

        _mockMongoCollection.Setup(m => m.FindAsync(
                It.IsAny<FilterDefinition<MongoEventStore.DomainEventSchema>>(),
                It.IsAny<FindOptions<MongoEventStore.DomainEventSchema, MongoEventStore.DomainEventSchema>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((FilterDefinition<MongoEventStore.DomainEventSchema> _, FindOptions<MongoEventStore.DomainEventSchema, MongoEventStore.DomainEventSchema> _, CancellationToken _) =>
                _mockAsyncCursor.Object);

        _mockAsyncCursor.Setup(m => m.MoveNext(It.IsAny<CancellationToken>()))
            .Returns((CancellationToken _) =>
                ++_moveNextAsyncCallCount <= 1);

        _mockAsyncCursor.Setup(m => m.Current)
            .Returns(_domainEvents);
    }

    [TearDown]
    public void TearDown()
    {
        _domainEvents.Clear();
        _moveNextAsyncCallCount = 0;

        _mockLogger.Invocations.Clear();
        _mockMongoClient.Invocations.Clear();
        _mockMongoDatabase.Invocations.Clear();
        _mockMongoCollection.Invocations.Clear();
        _mockAsyncCursor.Invocations.Clear();
    }

    [Test, Category(TestCategory.Unit)]
    public async Task GetEventStreamAsync_ReturnsDomainEvents_WhenEventStreamContainsDomainEvents()
    {
        // Arrange
        var testEvent = new TestEvent(
            Guid.NewGuid().ToString(),
            Guid.NewGuid().ToString(),
            1,
            true,
            DateTime.UtcNow);

        var domainEventSchemaId = new MongoEventStore.DomainEventSchemaId(testEvent.AggregateRootId, testEvent.Version);
        var domainEventSchema = new MongoEventStore.DomainEventSchema(domainEventSchemaId, testEvent);

        _domainEvents.Add(domainEventSchema);

        var mongoEventStore = new MongoEventStore(
            _mockLogger.Object,
            _mockMongoClient.Object,
            _mongoEventStoreSettings);

        // Act
        var actualDomainEvents = await mongoEventStore.GetEventStreamAsync(testEvent.AggregateRootId, 0, CancellationToken.None);

        // Assert
        actualDomainEvents
            .Should()
            .HaveCount(1)
            .And
            .ContainSingle(domainEvent => domainEvent == testEvent);

        _mockMongoCollection.Verify(m => m.FindAsync(
                It.IsAny<FilterDefinition<MongoEventStore.DomainEventSchema>>(),
                It.IsAny<FindOptions<MongoEventStore.DomainEventSchema, MongoEventStore.DomainEventSchema>>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}