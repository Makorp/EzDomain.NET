namespace EzDomain.EventSourcing.Domain.Model;

internal interface IAggregateRootBehavior
{
    void RestoreFromStream(IReadOnlyCollection<DomainEvent> eventStream);

    IReadOnlyCollection<DomainEvent> GetUncommittedChanges();

    void CommitChanges();
}