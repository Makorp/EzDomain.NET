using EzDomain.Core.Domain.Model;

namespace EzDomain.Core.Tests.UnitTests.TestDoubles;

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