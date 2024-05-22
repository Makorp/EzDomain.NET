using EzDomain.EventSourcing.Domain.Model;

namespace EzDomain.EventSourcing.EventStores.Azure.TableStorage.Tests.IntTests.TestDoubles;

internal sealed record StringEvent
    : DomainEvent
{
    public StringEvent(string aggregateRootId, string stringValue)
        : base(aggregateRootId)
    {
        StringValue = stringValue;
    }

    public string StringValue { get; }
}