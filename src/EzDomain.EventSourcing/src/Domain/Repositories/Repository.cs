using EzDomain.EventSourcing.Domain.EventStores;
using EzDomain.EventSourcing.Domain.Model;
using EzDomain.EventSourcing.Exceptions;

namespace EzDomain.EventSourcing.Domain.Repositories;

public sealed class Repository<TAggregateRoot, TAggregateRootId>
    : IRepository<TAggregateRoot, TAggregateRootId>
    where TAggregateRoot : AggregateRoot<TAggregateRootId>, IAggregateRoot<TAggregateRootId>, new()
    where TAggregateRootId : class, IAggregateRootId
{
    private readonly IEventStore _eventStore;

    public Repository(IEventStore eventStore) => _eventStore = eventStore;

    /// <summary>
    /// Gets an aggregate root with its correct state by the aggregate root identifier.
    /// </summary>
    /// <param name="aggregateRootId">Aggregate root identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Aggregate root.</returns>
    public async Task<TAggregateRoot?> GetByIdAsync(string aggregateRootId, CancellationToken cancellationToken = default)
    {
        var eventStream = await _eventStore.GetEventStreamAsync(aggregateRootId, Constants.InitialVersion, cancellationToken);
        if (!eventStream.Any())
            return default;

        var aggregateRoot = new TAggregateRoot();
        var aggregateRootBehavior = aggregateRoot.ToAggregateRootBehavior();

        aggregateRootBehavior.RestoreFromEventStream(eventStream);

        return aggregateRoot;
    }

    /// <summary>
    /// Saves state of an aggregate root in an event store and returns newly created domain events.
    /// </summary>
    /// <param name="aggregateRoot">Aggregate root.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Collection of newly created domain events.</returns>
    /// <exception cref="AggregateRootNullException">Thrown if aggregate root is null.</exception>
    public async Task<IReadOnlyCollection<DomainEvent>> SaveAsync(TAggregateRoot aggregateRoot, CancellationToken cancellationToken = default)
    {
        if (aggregateRoot is null)
            throw new AggregateRootNullException(nameof(aggregateRoot));

        var aggregateRootBehavior = aggregateRoot.ToAggregateRootBehavior();

        var changesToSave = aggregateRootBehavior.GetUncommittedChanges();
        if (!changesToSave.Any())
            return Array.Empty<DomainEvent>();

        aggregateRootBehavior.CommitChanges();

        await _eventStore.AppendToStreamAsync(changesToSave, cancellationToken);

        return changesToSave;
    }
}