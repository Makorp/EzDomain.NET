using EzDomain.Core.Domain.Model;

namespace EzDomain.Core.Domain.EventStores;

public interface IEventStore
{
    Task<IReadOnlyCollection<DomainEvent>> GetEventStreamAsync(string streamId, long fromVersion, CancellationToken cancellationToken = default);

    Task AppendToStreamAsync(IReadOnlyCollection<DomainEvent> events, CancellationToken cancellationToken = default);
}