using EzDomain.EventSourcing.Domain.Model;

namespace EzDomain.EventSourcing.Domain.Factories;

[ExcludeFromCodeCoverage]
public class AggregateRootFactory<TAggregateRoot, TAggregateRootId>
    : IAggregateRootFactory<TAggregateRoot, TAggregateRootId>
    where TAggregateRoot : class, IAggregateRoot<TAggregateRootId>, new()
    where TAggregateRootId : class, IAggregateRootId
{
    /// <summary>
    /// Restores aggregate root state from an event stream containing domain events.
    /// </summary>
    /// <returns>Aggregate root</returns>
    public virtual TAggregateRoot Create() => new();
}