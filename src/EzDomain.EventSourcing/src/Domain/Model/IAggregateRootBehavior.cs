namespace EzDomain.EventSourcing.Domain.Model;

internal interface IAggregateRootBehavior
{
    void RestoreFromEventStream(IReadOnlyCollection<DomainEvent> eventStream);

    IReadOnlyCollection<DomainEvent> GetUncommittedChanges();

    void CommitChanges();
}