using EzDomain.EventSourcing.Domain.Model;

namespace EzDomain.EventSourcing.EventStores.Azure.TableStorage.Tests.IntegrationTests.TestDoubles;

internal sealed record IntEvent
    : DomainEvent
{
    public IntEvent(string aggregateRootId, int intValue)
        : base(aggregateRootId)
    {
        IntValue = intValue;
    }

    public int IntValue { get; }
}