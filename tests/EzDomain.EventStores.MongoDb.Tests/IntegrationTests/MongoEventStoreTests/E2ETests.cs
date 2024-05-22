using EzDomain.EventStores.MongoDb.Tests.TestDoubles;

namespace EzDomain.EventStores.MongoDb.Tests.IntegrationTests.MongoEventStoreTests;

[TestFixture]
internal sealed class E2ETests
    : TestsBase
{
    [Test, Category(TestCategory.Integration)]
    public async Task GetEventStreamAsync_ReturnsDomainEvents_WhenAppendToStreamAsyncWasSuccessful()
    {
        // Arrange
        var aggregateRootId = Guid.NewGuid().ToString();

        var domainEvents = new List<DomainEvent>();
        for (var i = 0; i < 2; i++)
        {
            var streamSequenceNumber = i + 1;
            var testEvent = new TestEvent(
                aggregateRootId,
                $"StringValue_{streamSequenceNumber}",
                streamSequenceNumber,
                true,
                DateTime.UtcNow);

            testEvent.SetVersion(streamSequenceNumber);

            domainEvents.Add(testEvent);
        }

        // Act
        await Sut.AppendToStreamAsync(
            domainEvents,
            CancellationToken.None);

        var retrievedDomainEvents = await Sut.GetEventStreamAsync(
            aggregateRootId,
            0,
            CancellationToken.None);

        // Assert
        retrievedDomainEvents
            .Should()
            .BeEquivalentTo(domainEvents);
    }
}