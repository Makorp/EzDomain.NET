using EzDomain.EventSourcing.Domain.Model;

namespace EzDomain.EventSourcing.Serialization;

public interface IEventDataSerializer<TDomainEventDataSerializationType>
{
    /// <summary>
    /// Serializes domain event to serialization type.
    /// </summary>
    /// <param name="domainEvent">Domain event.</param>
    /// <returns>Serialized domain event to serialization type.</returns>
    TDomainEventDataSerializationType Serialize(DomainEvent domainEvent);

    /// <summary>
    /// Deserializes domain event from JSON string.
    /// </summary>
    /// <param name="obj">Serialization type.</param>
    /// <param name="typeName">Full name of domain event type.</param>
    /// <returns>Domain event object.</returns>
    DomainEvent Deserialize(TDomainEventDataSerializationType obj, string typeName);
}