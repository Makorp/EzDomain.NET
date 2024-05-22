using EzDomain.EventSourcing.Domain.Model;

namespace EzDomain.EventSourcing.EventStores.Azure.TableStorage.Tests.UnitTests.TestDoubles;

[Serializable]
internal sealed record FakeEvent
    : DomainEvent
{
    /// <inheritdoc/>
    public FakeEvent()
    {
    }

    /// <inheritdoc/>
    public FakeEvent(string aggregateRoodId)
        : base(aggregateRoodId)
    {
    }
}