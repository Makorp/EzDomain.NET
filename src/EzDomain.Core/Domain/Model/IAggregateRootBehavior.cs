namespace EzDomain.Core.Domain.Model;

internal interface IAggregateRootBehavior
{
    void RestoreFromEventStream(IReadOnlyCollection<DomainEvent> eventStream);

    IReadOnlyCollection<DomainEvent> GetUncommittedChanges();

    void CommitChanges();
}