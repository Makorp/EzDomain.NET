using EzDomain.EventSourcing.Domain.Model;

namespace EzDomain.EventSourcing.Tests.UnitTests.TestDoubles;

internal sealed record TestAggregateRootId
    : IAggregateRootId
{
    private readonly string _value;

    public TestAggregateRootId(string value)
        => _value = value;

    public override string ToString()
        => _value;
}