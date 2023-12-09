using EzDomain.Core.Domain.Model;

namespace EzDomain.Core.Tests.TestDoubles;

internal sealed record TestAggregateRootId
    : IAggregateRootId
{
    private readonly string _value;

    public TestAggregateRootId(string value)
        => _value = value;

    public override string ToString()
        => _value;
}