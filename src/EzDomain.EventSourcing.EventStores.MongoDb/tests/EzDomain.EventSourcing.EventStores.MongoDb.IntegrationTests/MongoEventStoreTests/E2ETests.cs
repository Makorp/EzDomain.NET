using EzDomain.EventSourcing.Domain.Model;
using EzDomain.EventSourcing.EventStores.MongoDb.IntegrationTests.TestDoubles;
using FluentAssertions;

namespace EzDomain.EventSourcing.EventStores.MongoDb.IntegrationTests.MongoEventStoreTests;

[TestFixture]
internal sealed class E2ETests
    : TestsBase
{
    [Test]
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
        await SystemUnderTest.AppendToStreamAsync(
            domainEvents,
            CancellationToken.None);

        var retrievedDomainEvents = await SystemUnderTest.GetEventStreamAsync(
            aggregateRootId,
            0,
            CancellationToken.None);

        // Assert
        retrievedDomainEvents
            .Should()
            .BeEquivalentTo(domainEvents);
    }
}