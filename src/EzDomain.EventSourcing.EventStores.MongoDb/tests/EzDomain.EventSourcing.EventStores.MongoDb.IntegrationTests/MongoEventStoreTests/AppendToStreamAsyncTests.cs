using EzDomain.EventSourcing.Domain.Model;
using EzDomain.EventSourcing.EventStores.MongoDb.IntegrationTests.TestDoubles;
using EzDomain.EventSourcing.Exceptions;
using FluentAssertions;

namespace EzDomain.EventSourcing.EventStores.MongoDb.IntegrationTests.MongoEventStoreTests;

[TestFixture]
internal sealed class AppendToStreamAsyncTests
    : TestsBase
{
    [Test]
    public Task AppendToStreamAsync_AppendsDomainEventsToTheEventStream_WhenEventStreamContainsNewDomainEvents()
    {
        // Arrange
        var domainEvents = SetDomainEvents().ToList();

        // Act
        var act = async () =>
        {
            await ActAsync(domainEvents, 1);
        };

        // Assert
        return act
            .Should()
            .NotThrowAsync();
    }

    [Test]
    public async Task AppendStreamAsync_ThrowsConcurrencyException_WhenDomainEventsWithTheSameVersionsExistInEventStream()
    {
        // Arrange
        var domainEvents = SetDomainEvents();

        // Act
        var act = async () =>
        {
            await ActAsync(domainEvents, 2);
        };

        // Assert
        await act
            .Should()
            .ThrowExactlyAsync<ConcurrencyException>();
    }

    private static IReadOnlyCollection<DomainEvent> SetDomainEvents()
    {
        var aggregateRootId = Guid.NewGuid().ToString();

        var testEvent = new TestEvent(
            aggregateRootId,
            Guid.NewGuid().ToString(),
            123,
            true,
            DateTime.UtcNow);

        testEvent.SetVersion(1);

        return new[]
        {
            testEvent
        };
    }

    private static async Task ActAsync(IReadOnlyCollection<DomainEvent> domainEvents, short numberOfAppendCalls)
    {
        for (var i = 0; i < numberOfAppendCalls; i++)
        {
            await SystemUnderTest.AppendToStreamAsync(
                domainEvents,
                CancellationToken.None);
        }
    }
}