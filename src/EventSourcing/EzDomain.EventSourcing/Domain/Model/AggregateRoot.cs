using EzDomain.EventSourcing.Exceptions;

namespace EzDomain.EventSourcing.Domain.Model;

public abstract class AggregateRoot<TId>
    : IAggregateRoot<TId>,
        IAggregateRootBehavior where TId : class, IAggregateRootId
{
    private readonly List<DomainEvent> _changes;

    private readonly List<MethodInfo> _eventListenerMethods;

    private TId? _id;

    /// <summary>
    /// Use this constructor only to restore aggregate root state from an event stream.
    /// </summary>
    protected AggregateRoot()
    {
        _changes = new List<DomainEvent>();

        _eventListenerMethods = InitializeEventListenerMethods();

        Version = Constants.InitialVersion;
    }

    /// <summary>
    /// Use this constructor only to create a new aggregate root.
    /// </summary>
    /// <param name="id">Aggregate Root Identifier</param>
    protected AggregateRoot(TId id)
        : this() => Id = id;

    [SuppressMessage("ReSharper", "JoinNullCheckWithUsage", Justification = "Left with if statement for readability")]
    public TId Id
    {
        get => _id!;
        protected set
        {
            if (_id is not null)
            {
                throw new AggregateRootIdException("Aggregate root identifier has been already initialized.");
            }

            if (value is null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            _id = value;
        }
    }

    public long Version { get; private set; }

    /// <summary>
    /// Restores an aggregate root state from an event stream containing domain events.
    /// </summary>
    /// <param name="eventStream">An event stream containing domain events.</param>
    /// <exception cref="EventStreamNullException">Thrown if event stream is null.</exception>
    /// <exception cref="EmptyEventStreamException">Thrown if event stream does not contain any domain events.</exception>
    void IAggregateRootBehavior.RestoreFromStream(IReadOnlyCollection<DomainEvent> eventStream)
    {
        if (eventStream is null)
        {
            throw new EventStreamNullException(nameof(eventStream));
        }

        if (!eventStream.Any())
        {
            throw new EmptyEventStreamException("Aggregate root events stream does not contain any domain events.");
        }

        var orderedEventStream = eventStream
            .OrderBy(e => e.Version)
            .ToList();

        orderedEventStream.ForEach(e => ApplyChange(e, false));

        var lastEvent = orderedEventStream.Last();

        Id = DeserializeIdFromString(lastEvent.AggregateRootId);
        Version = lastEvent.Version;
    }

    /// <summary>
    /// Gets uncommitted domain events.
    /// </summary>
    /// <returns>Read only collection of domain events.</returns>
    IReadOnlyCollection<DomainEvent> IAggregateRootBehavior.GetUncommittedChanges() => _changes.ToList();

    /// <summary>
    /// Commits changes made in aggregate root.
    /// </summary>
    /// <exception cref="AggregateRootIdException">Thrown if aggregate root identifier is null.</exception>
    void IAggregateRootBehavior.CommitChanges()
    {
        if (_id is null)
        {
            throw new AggregateRootIdException("Aggregate root identifier has not been initialized.");
        }

        var newAggregateRootVersion = Version;

        _changes.ForEach(domainEvent => domainEvent.IncrementVersion(ref newAggregateRootVersion));

        Version = newAggregateRootVersion;

        _changes.Clear();
    }

    /// <summary>
    /// Applies new domain event to aggregate root.
    /// </summary>
    /// <param name="domainEvent">Domain event</param>
    /// <exception cref="EventNullException">Thrown if domain event is null.</exception>
    protected void ApplyChange(DomainEvent domainEvent)
    {
        if (domainEvent is null)
        {
            throw new EventNullException(nameof(domainEvent));
        }

        ApplyChange(domainEvent, true);
    }

    /// <summary>
    /// Deserializes aggregate root identifier from string.
    /// </summary>
    /// <param name="serializedId">Serialized aggregate root identifier.</param>
    /// <returns>Deserialized aggregate root identifier.</returns>
    protected abstract TId DeserializeIdFromString(string serializedId);

    private List<MethodInfo> InitializeEventListenerMethods() =>
        GetType()
            .GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
            .Where(mi => mi.Name == "On")
            .ToList();

    private void ApplyChange(DomainEvent domainEvent, bool isNew)
    {
        InvokeEventListenerMethod(domainEvent);

        if (isNew)
        {
            _changes.Add(domainEvent);
        }
    }

    private void InvokeEventListenerMethod(DomainEvent domainEvent)
    {
        var eventListenerMethod = _eventListenerMethods.SingleOrDefault(methodInfo => methodInfo.GetParameters().Single().ParameterType == domainEvent.GetType());
        if (eventListenerMethod is null)
        {
            throw new MissingMethodException($"Event listener method was not found for the {domainEvent.GetType().Name} event.");
        }

        eventListenerMethod.Invoke(this, new object[] { domainEvent });
    }
}