using EzDomain.Core.Domain.Model;

namespace EzDomain.Core.Serialization;

/// <summary>
/// JSON serializer for domain events.
/// </summary>
public class JsonDomainEventSerializer
    : IDomainEventSerializer<string>
{
    /// <summary>
    /// Serializes a domain event to a JSON string.
    /// </summary>
    /// <param name="domainEvent">Domain event.</param>
    /// <returns>Serialized domain event.</returns>
    /// <exception cref="ArgumentNullException">Thrown if a domain event is null.</exception>
    public virtual string Serialize(DomainEvent? domainEvent)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);

        return JsonSerializer.Serialize(domainEvent, domainEvent.GetType());
    }

    /// <summary>
    /// Deserializes a domain event from a JSON string.
    /// </summary>
    /// <param name="jsonString">JSON string.</param>
    /// <param name="domainEventTypeFullName">Full name of a domain event type.</param>
    /// <returns>Domain event object.</returns>
    /// <exception cref="InvalidOperationException">Thrown if type of a domain event does not exist.</exception>
    public virtual DomainEvent? Deserialize(string? jsonString, string? domainEventTypeFullName)
    {
        if (string.IsNullOrWhiteSpace(jsonString))
            throw new ArgumentNullException(nameof(jsonString));

        if (string.IsNullOrWhiteSpace(domainEventTypeFullName))
            throw new ArgumentNullException(nameof(domainEventTypeFullName));

        var domainEventType = Type.GetType(domainEventTypeFullName);
        if (domainEventType is null)
            throw new InvalidOperationException("Provided domain event type full name was not found.");

        var jsonNode = JsonNode.Parse(jsonString)!;

        var version = jsonNode["Version"]!.GetValue<long>();

        using var memoryStream = new MemoryStream();
        using var streamWriter = new StreamWriter(memoryStream);

        streamWriter.Write(jsonString);
        streamWriter.Flush();

        memoryStream.Position = 0;

        // TODO: Consider using .Deserialize<T>
        var domainEvent = JsonSerializer.Deserialize(memoryStream, domainEventType);

        var versionField = typeof(DomainEvent).GetField("_version", BindingFlags.Instance | BindingFlags.NonPublic);
        versionField?.SetValue(domainEvent, version);

        return domainEvent! as DomainEvent;
    }
}