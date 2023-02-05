namespace EzDomain.EventSourcing.Domain.Model;

public interface IAggregateRoot<out TId>
    where TId : class, IAggregateRootId
{
    TId Id { get; }
        
    long Version { get; }
}