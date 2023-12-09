using EzDomain.EventSourcing.Domain.Model;

namespace EzDomain.EventSourcing.EventStores.SqlServer.IntegrationTests.TestDoubles;

internal sealed record TestEvent
    : DomainEvent
{
    public TestEvent()
    {
    }

    public TestEvent(string aggregateId)
        : base(aggregateId)
    {
    }
}