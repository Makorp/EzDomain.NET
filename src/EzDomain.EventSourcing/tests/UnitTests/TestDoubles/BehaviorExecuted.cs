using EzDomain.EventSourcing.Domain.Model;

namespace EzDomain.EventSourcing.Tests.UnitTests.TestDoubles;

public sealed record BehaviorExecuted
    : DomainEvent
{
    /// <inheritdoc/>
    public BehaviorExecuted()
    {
    }

    /// <inheritdoc/>
    public BehaviorExecuted(string aggregateRootId)
        : base(aggregateRootId)
    {
    }
}