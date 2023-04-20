using EzDomain.EventSourcing.Domain.EventStores;
using EzDomain.EventSourcing.Domain.Model;
using Microsoft.Extensions.Logging;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;

namespace EzDomain.EventSourcing.EventStores.MongoDbEventStore;

public sealed class MongoDbStore
    : EventStore
{
    private readonly MongoClient _mongoClient;

    public MongoDbStore(ILogger logger, MongoClient mongoClient)
        : base(logger)
    {
        _mongoClient = mongoClient;
    }

    public override async Task<IReadOnlyCollection<DomainEvent>> GetEventStreamAsync(string aggregateRootId, long fromVersion, CancellationToken cancellationToken = default) =>
        await _mongoClient
            .GetDatabase("EventStore")
            .GetCollection<DomainEvent>("Events")
            .Find(domainEvent =>
                domainEvent.AggregateRootId == aggregateRootId &&
                domainEvent.Version >= fromVersion)
            .ToListAsync(cancellationToken);

    protected override async Task AppendToStreamInternalAsync(IReadOnlyCollection<DomainEvent> events,
        CancellationToken cancellationToken = default)
    {
        if (events is null)
            throw new ArgumentNullException(nameof(events));

        var eventSchemas = events.Select(domainEvent => new Schema
        (
            domainEvent.AggregateRootId!,
            domainEvent.Version,
            domainEvent.GetType().FullName!,
            domainEvent
        ));

        await _mongoClient
            .GetDatabase("EventStore")
            .GetCollection<Schema>("Events")
            .InsertManyAsync(eventSchemas, cancellationToken: cancellationToken);
    }

    protected override bool IsConcurrencyException(Exception ex)
    {
        throw new NotImplementedException();
    }

    private sealed class Schema
    {
        [BsonConstructor]
        public Schema(string streamId, long streamVersion, string eventType, DomainEvent eventData)
        {
            StreamId = streamId;
            StreamVersion = streamVersion;

            EventType = eventType;
            EventData = eventData;
        }

        public string StreamId { get; set; }

        public long StreamVersion { get; set; }

        public string EventType { get; set; }

        public DomainEvent EventData { get; set; }
    }
}