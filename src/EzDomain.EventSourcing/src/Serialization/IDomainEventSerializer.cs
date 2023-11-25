using EzDomain.EventSourcing.Domain.Model;

namespace EzDomain.EventSourcing.Serialization;

public interface IDomainEventSerializer<TDomainEventDataSerializationType>
{
    /// <summary>
    /// Serializes a domain event to a serialization type.
    /// </summary>
    /// <param name="domainEvent">Domain event.</param>
    /// <returns>Serialized domain event.</returns>
    TDomainEventDataSerializationType Serialize(DomainEvent? domainEvent);

    /// <summary>
    /// Deserializes a domain event from a JSON string.
    /// </summary>
    /// <param name="obj">Serialization type.</param>
    /// <param name="domainEventTypeFullName">Full name of a domain event type.</param>
    /// <returns>Domain event object.</returns>
    DomainEvent? Deserialize(TDomainEventDataSerializationType? obj, string? domainEventTypeFullName);
}