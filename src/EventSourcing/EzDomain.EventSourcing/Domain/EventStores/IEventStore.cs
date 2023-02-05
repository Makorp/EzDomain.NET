using EzDomain.EventSourcing.Domain.Model;

namespace EzDomain.EventSourcing.Domain.EventStores;

public interface IEventStore
{
    Task<IReadOnlyCollection<DomainEvent>> GetByAggregateRootIdAsync(string aggregateRootId, long fromVersion, CancellationToken cancellationToken = default);

    Task SaveAsync(IReadOnlyCollection<DomainEvent> events, CancellationToken cancellationToken = default);
}