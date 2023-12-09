using EzDomain.Core.Exceptions;
using EzDomain.Core.Tests.TestDoubles;

namespace EzDomain.Core.Tests.Domain.Model;

[TestFixture]
public sealed class DomainEventTests
{
    [Test]
    public void CallingDefaultDomainEventConstructor_InitializesDefaultDomainEventVersion()
    {
        // Arrange
        var domainEvent = new BehaviorExecuted();

        // Assert
        domainEvent.AggregateRootId
            .Should()
            .BeNull();

        domainEvent.Version
            .Should()
            .Be(Constants.InitialVersion);
    
    }

    [Test]
    [AutoData]
    public void CallingDomainEventConstructor_InitializesCorrectDomainEventState_WhenAggregateRootIdIsGiven(string aggregateRootId)
    {
        // Act
        var domainEvent = new BehaviorExecuted(aggregateRootId);

        // Assert
        domainEvent.AggregateRootId
            .Should()
            .BeEquivalentTo(aggregateRootId);

        domainEvent.Version
            .Should()
            .Be(Constants.InitialVersion);
    }

    [TestCase("")]
    [TestCase(" ")]
    [TestCase(null)]
    public void CallingDomainEventConstructor_ThrowsAggregateRootIdException_WhenAggregateRootIdIsNullOrWhitespace(string aggregateRootId)
    {
        // Act
        var action = () => new BehaviorExecuted(aggregateRootId);

        // Assert
        action
            .Should()
            .Throw<AggregateRootIdException>();
    }

    [Test]
    [AutoData]
    public void IncrementVersion_IncrementsAggregateRootVersionAndSetsEventVersion_WhenAggregateRootVersionIsValid(string aggregateRootId)
    {
        // Arrange
        var aggregateRootVersion = Constants.InitialVersion;

        const long expectedAggregateRootVersion = Constants.InitialVersion + 1;

        // Act
        var domainEvent = new BehaviorExecuted(aggregateRootId);

        domainEvent.IncrementVersion(ref aggregateRootVersion);

        // Assert
        aggregateRootVersion
            .Should()
            .Be(expectedAggregateRootVersion);

        domainEvent.Version
            .Should()
            .Be(expectedAggregateRootVersion);
    }

    [Test]
    [AutoData]
    public void IncrementVersion_ThrowsAggregateRootVersionException_WhenAggregateRootVersionIsInvalid(string aggregateRootId)
    {
        // Arrange
        long aggregateRootVersion = -2;

        // Act
        var domainEvent = new BehaviorExecuted(aggregateRootId);

        var action = () => domainEvent.IncrementVersion(ref aggregateRootVersion);

        // Assert
        action
            .Should()
            .Throw<AggregateRootVersionException>();
    }
}