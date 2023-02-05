using EzDomain.EventSourcing.Domain.Model;

namespace EzDomain.EventSourcing.Domain.EventStores;

public abstract class EventStore
    : IEventStore
{
    public abstract Task<IReadOnlyCollection<DomainEvent>> GetByAggregateRootIdAsync(string aggregateRootId, long fromVersion, CancellationToken cancellationToken = default);

    public abstract Task SaveAsync(IReadOnlyCollection<DomainEvent> events, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if concurrency occured while saving domain events to event store.
    /// </summary>
    /// <param name="ex">Exception that occured in event store.</param>
    /// <returns>Returns true if exception was caused by domain events concurrency.</returns>
    protected abstract bool IsConcurrencyException(Exception ex);
}