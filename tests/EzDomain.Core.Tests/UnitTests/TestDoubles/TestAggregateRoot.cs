using EzDomain.Core.Domain.Model;

namespace EzDomain.Core.Tests.UnitTests.TestDoubles;

internal sealed class TestAggregateRoot
    : AggregateRoot<TestAggregateRootId>, ITestAggregateRoot
{
    ///<inheritdoc/>
    public TestAggregateRoot()
    {
    }

    ///<inheritdoc/>
    public TestAggregateRoot(TestAggregateRootId id)
        : base(id)
    {
    }

    public void SetId(string id)
        => Id = new TestAggregateRootId(id);

    public void ExecuteBehavior()
        => ApplyChange(new BehaviorExecuted(Id.ToString()));

    public void ExecuteUnhandledBehavior()
        => ApplyChange(new UnhandledBehaviorExecuted(Id.ToString()));

    protected override TestAggregateRootId DeserializeIdFromString(string serializedId)
        => new(serializedId);

    private void On(BehaviorExecuted domainEvent)
    {
    }
}