using EzDomain.EventSourcing.Domain.Model;

namespace EzDomain.EventSourcing.Domain.Factories;

public interface IAggregateRootFactory<out TAggregateRoot, TAggregateRootId>
    where TAggregateRoot : IAggregateRoot<TAggregateRootId>
    where TAggregateRootId : class, IAggregateRootId
{
    /// <summary>
    /// Restores aggregate root state from an event stream containing domain events.
    /// </summary>
    /// <returns>Aggregate root</returns>
    TAggregateRoot Create();
}