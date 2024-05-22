using EzDomain.Core.Domain.Model;

namespace EzDomain.Core.Tests.UnitTests.TestDoubles;

internal interface ITestAggregateRoot
    : IAggregateRoot<TestAggregateRootId>
{
    public void ExecuteBehavior();
        
    public void ExecuteUnhandledBehavior();
}