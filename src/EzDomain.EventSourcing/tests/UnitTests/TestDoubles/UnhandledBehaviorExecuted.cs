using EzDomain.EventSourcing.Domain.Model;

namespace EzDomain.EventSourcing.Tests.UnitTests.TestDoubles;

internal sealed record UnhandledBehaviorExecuted
    : DomainEvent
{
    public UnhandledBehaviorExecuted(string aggregateRootId)
        : base(aggregateRootId)
    {
    }
}