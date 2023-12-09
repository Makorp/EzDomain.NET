using EzDomain.EventStores.MongoDb.Tests.TestDoubles;

namespace EzDomain.EventStores.MongoDb.Tests.UnitTests.MongoEventStoreTests;

[TestFixture]
internal sealed class AppendToStreamAsyncTests
{
    private readonly List<MongoEventStore.DomainEventSchema> _domainEvents = new();

    private readonly Mock<ILogger> _mockLogger = new();
    private readonly Mock<IMongoClient> _mockMongoClient = new();
    private readonly Mock<IMongoDatabase> _mockMongoDatabase = new();
    private readonly Mock<IMongoCollection<MongoEventStore.DomainEventSchema>> _mockMongoCollection = new();

    private readonly MongoEventStoreSettings _mongoEventStoreSettings = new(
        $"dummyEventStore",
        $"domainEvents",
        new[]
        {
            typeof(TestEvent)
        });

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

        _mockMongoCollection.Setup(m => m.InsertManyAsync(
                It.IsAny<IEnumerable<MongoEventStore.DomainEventSchema>>(),
                It.IsAny<InsertManyOptions>(),
                It.IsAny<CancellationToken>()))
            .Callback((IEnumerable<MongoEventStore.DomainEventSchema> domainEvents, InsertManyOptions _, CancellationToken _) =>
                _domainEvents.AddRange(domainEvents));
    }

    [TearDown]
    public void TearDown()
    {
        _domainEvents.Clear();

        _mockLogger.Invocations.Clear();
        _mockMongoClient.Invocations.Clear();
        _mockMongoDatabase.Invocations.Clear();
        _mockMongoCollection.Invocations.Clear();
    }


    [Test, Category(TestCategory.Unit)]
    public async Task AppendToStreamAsync_AppendsDomainEventsToTheEventStream_WhenEventStreamContainsNewDomainEvents()
    {
        // Arrange
        var mongoEventStore = new MongoEventStore(
            _mockLogger.Object,
            _mockMongoClient.Object,
            _mongoEventStoreSettings);

        var testEvent = new TestEvent(
            Guid.NewGuid().ToString(),
            Guid.NewGuid().ToString(),
            1,
            true,
            DateTime.UtcNow);

        testEvent.SetVersion(1);

        var domainEvents = new DomainEvent[]
        {
            testEvent
        };

        // Act
        await mongoEventStore.AppendToStreamAsync(domainEvents, CancellationToken.None);

        // Assert
        _domainEvents
            .Should()
            .HaveCount(1);

        var savedDomainEvent = _domainEvents.First();

        savedDomainEvent.Id.StreamId
            .Should()
            .Be(testEvent.AggregateRootId);

        savedDomainEvent.Id.StreamSequenceNumber
            .Should()
            .Be(testEvent.Version);

        savedDomainEvent.EventData
            .Should()
            .BeEquivalentTo(testEvent);

        _mockMongoCollection
            .Verify(x => x.InsertManyAsync(
                It.Is<IEnumerable<MongoEventStore.DomainEventSchema>>(y => y.Count() == 1),
                It.IsAny<InsertManyOptions>(),
                It.IsAny<CancellationToken>()),
                Times.Once);
    }
}