using EzDomain.Core.Domain.Model;

namespace EzDomain.Core.Tests.TestDoubles;

internal interface ITestAggregateRoot
    : IAggregateRoot<TestAggregateRootId>
{
    public void ExecuteBehavior();
        
    public void ExecuteUnhandledBehavior();
}