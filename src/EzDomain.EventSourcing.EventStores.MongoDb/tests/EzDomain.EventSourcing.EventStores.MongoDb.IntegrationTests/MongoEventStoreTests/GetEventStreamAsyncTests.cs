using EzDomain.EventSourcing.EventStores.MongoDb.IntegrationTests.TestDoubles;
using FluentAssertions;
using MongoDB.Bson;

namespace EzDomain.EventSourcing.EventStores.MongoDb.IntegrationTests.MongoEventStoreTests;

[TestFixture]
internal sealed class GetEventStreamAsyncTests
    : TestsBase
{
    private readonly IReadOnlyCollection<string> _streamIds = new[]
    {
        Guid.NewGuid().ToString(),
        Guid.NewGuid().ToString(),
        Guid.NewGuid().ToString()
    };

    [OneTimeSetUp]
    public override void OneTimeSetUp()
    {
        var documents = new List<BsonDocument>();
        
        foreach (var streamId in _streamIds)
        {
            for (var i = 0; i < 2; i++)
            {
                var streamSequenceNumber = i + 1;

                var testEvent = new TestEvent(streamId, "StringValue", streamSequenceNumber, true, DateTime.UtcNow);
                testEvent.SetVersion(streamSequenceNumber);

                var document = new BsonDocument
                {
                    {
                        "_id", new BsonDocument
                        {
                            { "StreamId", streamId },
                            { "StreamSequenceNumber", streamSequenceNumber }
                        }
                    },
                    { "EventData", testEvent.ToBsonDocument() }
                };

                documents.Add(document);
            }
        }

        MongoClient
            .GetDatabase(MongoEventStoreSettings.DatabaseName)
            .GetCollection<BsonDocument>(MongoEventStoreSettings.CollectionName)
            .InsertMany(documents);
    }

    [Test]
    public async Task GetEventStreamAsync_ReturnsDomainEvents_WhenStreamIdForExistingEventStreamIsProvided()
    {
        // Act
        var eventStream = await SystemUnderTest.GetEventStreamAsync(
            _streamIds.First(),
            0,
            CancellationToken.None);

        // Assert
        eventStream.Count
            .Should()
            .Be(2);
    }
}