using EzDomain.Core.Exceptions;

namespace EzDomain.Core.Domain.Model;

/// <summary>
/// Base class for a domain event created by an aggregate root.
/// </summary>
public abstract record DomainEvent
{
    private long _version;

    /// <summary>
    /// Use this constructor only for deserialization of a domain event.
    /// </summary>
#pragma warning disable CS8618
    protected DomainEvent()
        => _version = Constants.InitialVersion;
#pragma warning restore CS8618

    /// <summary>
    /// Use this constructor only for creation of a new domain event.
    /// </summary>
    /// <param name="aggregateRootId">Serialized aggregate root identifier.</param>
    /// <exception cref="AggregateRootIdException">Thrown if serialized aggregate root identifier is null, empty or whitespace.</exception>
    protected DomainEvent(string aggregateRootId)
        : this()
    {
        if (string.IsNullOrWhiteSpace(aggregateRootId))
            throw new AggregateRootIdException("Serialized aggregate root identifier cannot be null, empty or whitespace.");

        AggregateRootId = aggregateRootId;
    }

    /// <summary>
    /// Serialized aggregate root identifier.
    /// </summary>
    public string AggregateRootId { get; }

    /// <summary>
    /// Event version.
    /// </summary>
    public long Version => _version; // TODO: Consider naming it AggregateRootVersion or SequenceNumber.

    /// <summary>
    /// Increments version of a domain event.
    /// </summary>
    /// <param name="aggregateRootVersion">Current aggregate root version.</param>
    /// <param name="initialVersion">Initial version of an aggregate root.</param>
    /// <exception cref="AggregateRootVersionException">Thrown if provided aggregate root version is less than 0 (initial aggregate root version).</exception>
    internal void IncrementVersion(ref long aggregateRootVersion, long initialVersion = Constants.InitialVersion)
    {
        if (aggregateRootVersion < initialVersion)
            throw new AggregateRootVersionException($"Aggregate root version must be greater or equal to {Constants.InitialVersion}, but was {aggregateRootVersion}.");

        _version = ++aggregateRootVersion;
    }
}