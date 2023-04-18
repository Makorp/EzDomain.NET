using EzDomain.EventSourcing.Domain.Model;

namespace EzDomain.EventSourcing.Domain.Repositories;

public interface IRepository<TAggregateRoot, in TAggregateRootId>
    where TAggregateRoot : class, IAggregateRoot<TAggregateRootId>
    where TAggregateRootId : class, IAggregateRootId
{
    /// <summary>
    /// Gets aggregate root by its identifier.
    /// </summary>
    /// <param name="aggregateRootId">Aggregate root identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Aggregate root.</returns>
    Task<TAggregateRoot?> GetByIdAsync(string aggregateRootId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves state of aggregate root in event store and returns newly stored domain events.
    /// </summary>
    /// <param name="aggregateRoot">Aggregate root.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Collection of newly stored domain events.</returns>
    Task<IReadOnlyCollection<DomainEvent>> SaveAsync(TAggregateRoot aggregateRoot, CancellationToken cancellationToken = default);
}