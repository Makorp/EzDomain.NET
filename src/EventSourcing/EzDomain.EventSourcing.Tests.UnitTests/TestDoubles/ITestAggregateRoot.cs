using EzDomain.EventSourcing.Domain.Model;

namespace EzDomain.EventSourcing.Tests.UnitTests.TestDoubles;

internal interface ITestAggregateRoot
    : IAggregateRoot<TestAggregateRootId>
{
    public void ExecuteBehavior();
        
    public void ExecuteUnhandledBehavior();
}