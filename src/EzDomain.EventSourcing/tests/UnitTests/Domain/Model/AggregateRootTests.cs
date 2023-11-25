using System.Linq;
using EzDomain.EventSourcing.Domain.Model;
using EzDomain.EventSourcing.Exceptions;
using EzDomain.EventSourcing.Tests.UnitTests.TestDoubles;

namespace EzDomain.EventSourcing.Tests.UnitTests.Domain.Model;

[TestFixture]
public sealed class AggregateRootTests
{
    [Test]
    [AutoData]
    public void CallingAggregateRootConstructor_InitializesAggregateRootWithCorrectState_WhenAggregateRootIdentifierIsGiven(string serializedAggregateRootId)
    {
        // Act
        var aggregateRootId = new TestAggregateRootId(serializedAggregateRootId);
        var aggregateRoot = new TestAggregateRoot(aggregateRootId);
            
        // Assert
        aggregateRoot.Id
            .Should()
            .NotBeNull();

        aggregateRoot.Id.ToString()
            .Should()
            .Be(serializedAggregateRootId);

        aggregateRoot.Version
            .Should()
            .Be(Constants.InitialVersion);
    }

    [Test]
    [AutoData]
    public void RestoreFromStream_SetsAggregateRootCorrectState_WhenDomainEventStreamIsProvided(string serializedAggregateRootId)
    {
        // Arrange
        const int expectedNumberOfUncommittedEvents = 0;
            
        var eventVersion = Constants.InitialVersion;

        var domainEvent = new BehaviorExecuted(serializedAggregateRootId);
        domainEvent.IncrementVersion(ref eventVersion);

        var eventsStream = new List<DomainEvent>
        {
            domainEvent
        };

        var aggregateRoot = new TestAggregateRoot();
        var aggregateRootBehavior = (IAggregateRootBehavior)aggregateRoot;

        // Act
        aggregateRootBehavior.RestoreFromEventStream(eventsStream);

        // Assert
        aggregateRoot.Id.ToString()
            .Should()
            .Be(serializedAggregateRootId);

        aggregateRoot.Version
            .Should()
            .Be(eventVersion)
            .And
            .BeGreaterThan(Constants.InitialVersion);

        aggregateRootBehavior.GetUncommittedChanges().Count
            .Should()
            .Be(expectedNumberOfUncommittedEvents);
    }

    [Test]
    public void RestoreFromStream_ThrowsEmptyEventStreamException_WhenDomainEventStreamContainsNoEvents()
    {
        // Arrange
        var aggregateRoot = new TestAggregateRoot();

        // Act
        var act = () => ((IAggregateRootBehavior)aggregateRoot).RestoreFromEventStream(Array.Empty<DomainEvent>());

        // Assert
        act
            .Should()
            .Throw<EventStreamEmptyException>();

        aggregateRoot.Id
            .Should()
            .BeNull();

        aggregateRoot.Version
            .Should()
            .Be(Constants.InitialVersion);
    }

    [Test]
    [AutoData]
    public void CommitChanges_SetsCorrectAggregateRootVersion_WhenAggregateRootBehaviorWasExecuted(string serializedAggregateRootId)
    {
        // Arrange
        const int expectedNumberOfChangesToSave = 1;
        const int expectedVersion = 1;
        const int expectedNumberOfUncommittedChanges = 0;

        var aggregateRootId = new TestAggregateRootId(serializedAggregateRootId);
        var aggregateRoot = new TestAggregateRoot(aggregateRootId);
        var aggregateRootBehavior = (IAggregateRootBehavior)aggregateRoot;

        // Act
        aggregateRoot.ExecuteBehavior();

        var changesToSave = aggregateRootBehavior.GetUncommittedChanges().ToArray();

        aggregateRootBehavior.CommitChanges();

        // Assert
        foreach (var domainEvent in changesToSave)
        {
            domainEvent.Version
                .Should()
                .Be(expectedVersion);
        }

        changesToSave.Length
            .Should()
            .Be(expectedNumberOfChangesToSave);

        aggregateRoot.Version
            .Should()
            .Be(expectedVersion);

        aggregateRootBehavior.GetUncommittedChanges().Count
            .Should()
            .Be(expectedNumberOfUncommittedChanges);
    }

    [Test]
    public void CommitChanges_ThrowsAggregateRootIdException_WhenAggregateRootHasNoIdSet()
    {
        // Arrange
        var aggregateRoot = new TestAggregateRoot();

        // Act
        var act = () => ((IAggregateRootBehavior)aggregateRoot).CommitChanges();

        // Assert
        act
            .Should()
            .Throw<AggregateRootIdException>();

        aggregateRoot.Version
            .Should()
            .Be(Constants.InitialVersion);
    }

    [Test]
    [AutoData]
    // TODO: Consider new exception: BehaviorExecutionFailedException + inner exception
    public void ExecutingAggregateRootBehavior_ThrowsMissingMethodException_WhenDomainEventHandlerIsNotImplemented(string serializedAggregateRootId)
    {
        // Arrange
        var aggregateRootId = new TestAggregateRootId(serializedAggregateRootId);
        var aggregateRoot = new TestAggregateRoot(aggregateRootId);

        // Act
        var act = () => aggregateRoot.ExecuteUnhandledBehavior();
            
        // Assert
        act
            .Should()
            .Throw<MissingMethodException>();
    }

    [Test]
    [AutoData]
    public void MultipleAggregateRootIdSetAttempts_ThrowsAggregateRootIdException_WhenAggregateRootIdentifierIsAlreadySet(string serializedAggregateRootId)
    {
        // Arrange
        var aggregateRootId = new TestAggregateRootId(serializedAggregateRootId);
        var aggregateRoot = new TestAggregateRoot(aggregateRootId);
            
        // Act
        var act = () => aggregateRoot.SetId(serializedAggregateRootId);
            
        // Assert
        act
            .Should()
            .Throw<AggregateRootIdException>();
    }
}