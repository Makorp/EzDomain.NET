using EzDomain.EventSourcing.Domain.Model;

namespace EzDomain.EventSourcing.Serialization;

/// <summary>
/// JSON serializer for domain events.
/// </summary>
public sealed class JsonEventDataSerializer
    : IEventDataSerializer<string>
{
    /// <summary>
    /// Serializes domain event to JSON string.
    /// </summary>
    /// <param name="domainEvent">Domain event.</param>
    /// <typeparam name="TDomainEvent">Domain event type.</typeparam>
    /// <returns>Serialized domain event to JSON string.</returns>
    public string Serialize<TDomainEvent>(TDomainEvent domainEvent)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);

        return JsonSerializer.Serialize(domainEvent);
    }

    /// <summary>
    /// Deserializes domain event from JSON string.
    /// </summary>
    /// <param name="jsonString">JSON string.</param>
    /// <param name="typeName">Full name of domain event type.</param>
    /// <returns>Domain event object.</returns>
    /// <exception cref="InvalidOperationException">Thrown if type of domain event does not exist.</exception>
    public DomainEvent Deserialize(string jsonString, string typeName)
    {
        var eventType = Type.GetType(typeName);
        if (eventType is null)
        {
            throw new InvalidOperationException("Provided type is incorrect");
        }

        var jsonNode = JsonNode.Parse(jsonString)!;

        var version = jsonNode["Version"]!.GetValue<long>();

        using var memoryStream = new MemoryStream();
        using var streamWriter = new StreamWriter(memoryStream);

        streamWriter.Write(jsonString);
        streamWriter.Flush();

        memoryStream.Position = 0;

        // TODO: Consider using .Deserialize<T>
        var domainEvent = JsonSerializer.Deserialize(memoryStream, eventType);

        var versionField = typeof(DomainEvent).GetField("_version", BindingFlags.Instance | BindingFlags.NonPublic);
        versionField?.SetValue(domainEvent, version);

        return domainEvent! as DomainEvent;
    }
}