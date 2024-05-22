using EzDomain.Core.Exceptions;
using EzDomain.EventStores.MongoDb.Tests.TestDoubles;

namespace EzDomain.EventStores.MongoDb.Tests.IntegrationTests.MongoEventStoreTests;

[TestFixture]
internal sealed class AppendToStreamAsyncTests
    : TestsBase
{
    [Test, Category(TestCategory.Integration)]
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

    [Test, Category(TestCategory.Integration)]
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
            await Sut.AppendToStreamAsync(
                domainEvents,
                CancellationToken.None);
        }
    }
}