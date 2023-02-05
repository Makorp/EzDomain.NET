using EzDomain.EventSourcing.Domain.EventStores;
using EzDomain.EventSourcing.Domain.Factories;
using EzDomain.EventSourcing.Domain.Model;
using EzDomain.EventSourcing.Exceptions;

namespace EzDomain.EventSourcing.Domain.Repositories;

public sealed class Repository<TAggregateRoot, TAggregateRootId>
    : IRepository<TAggregateRoot, TAggregateRootId>
    where TAggregateRoot : class, IAggregateRoot<TAggregateRootId>
    where TAggregateRootId : class, IAggregateRootId
{
    private readonly IAggregateRootFactory<TAggregateRoot, TAggregateRootId> _factory;
    private readonly IEventStore _eventStore;

    public Repository(IAggregateRootFactory<TAggregateRoot, TAggregateRootId> factory, IEventStore eventStore)
    {
        _factory = factory;
        _eventStore = eventStore;
    }

    /// <summary>
    /// Gets aggregate root with its correct state by aggregate root identifier.
    /// </summary>
    /// <param name="aggregateRootId">Aggregate root identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Aggregate root.</returns>
    public async Task<TAggregateRoot> GetByIdAsync(string aggregateRootId, CancellationToken cancellationToken = default)
    {
        var eventStream = await _eventStore.GetByAggregateRootIdAsync(aggregateRootId, Constants.InitialVersion, cancellationToken);
        if (!eventStream.Any())
        {
            return default;
        }

        var aggregateRoot = _factory.Create();
        var aggregateRootBehavior = CastToBehavior(aggregateRoot);

        aggregateRootBehavior.RestoreFromStream(eventStream);

        return aggregateRoot;
    }

    /// <summary>
    /// Saves state of aggregate root in event store and returns newly stored domain events.
    /// </summary>
    /// <param name="aggregateRoot">Aggregate root.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Collection of newly stored domain events.</returns>
    /// <exception cref="AggregateRootNullException">Thrown if aggregate root is null.</exception>
    public async Task<IReadOnlyCollection<DomainEvent>> SaveAsync(TAggregateRoot aggregateRoot, CancellationToken cancellationToken = default)
    {
        if (aggregateRoot is null)
        {
            throw new AggregateRootNullException(nameof(aggregateRoot));
        }

        var aggregateRootBehavior = CastToBehavior(aggregateRoot);

        var changesToSave = aggregateRootBehavior.GetUncommittedChanges();
        if (!changesToSave.Any())
        {
            return Array.Empty<DomainEvent>();
        }

        aggregateRootBehavior.CommitChanges();

        await _eventStore.SaveAsync(changesToSave, cancellationToken);

        return changesToSave;
    }

    private static IAggregateRootBehavior CastToBehavior(TAggregateRoot aggregateRoot)
    {
        if (aggregateRoot is not IAggregateRootBehavior aggregateRootBehavior)
        {
            throw new InvalidCastException("Aggregate root must implement AggregateRoot abstract class.");
        }

        return aggregateRootBehavior;
    }
}