using EzDomain.Core.Domain.Model;

namespace EzDomain.Core.Tests.UnitTests.TestDoubles;

internal sealed record UnhandledBehaviorExecuted
    : DomainEvent
{
    public UnhandledBehaviorExecuted(string aggregateRootId)
        : base(aggregateRootId)
    {
    }
}