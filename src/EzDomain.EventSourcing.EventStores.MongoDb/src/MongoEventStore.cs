using EzDomain.EventSourcing.Domain.EventStores;
using EzDomain.EventSourcing.Domain.Model;
using Microsoft.Extensions.Logging;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;

namespace EzDomain.EventSourcing.EventStores.MongoDb;

public sealed class MongoEventStore
    : EventStore
{
    private readonly MongoClient _mongoClient;
    private readonly MongoEventStoreSettings _mongoSettings;

    public MongoEventStore(ILogger logger, MongoClient mongoClient, MongoEventStoreSettings mongoSettings)
        : base(logger)
    {
        _mongoClient = mongoClient;
        _mongoSettings = mongoSettings;
    }

    public override async Task<IReadOnlyCollection<DomainEvent>> GetEventStreamAsync(string aggregateRootId, long fromVersion, CancellationToken cancellationToken = default) =>
        await _mongoClient
            .GetDatabase(_mongoSettings.DatabaseName)
            .GetCollection<DomainEvent>(_mongoSettings.StreamName)
            .Find(domainEvent =>
                domainEvent.AggregateRootId == aggregateRootId &&
                domainEvent.Version >= fromVersion)
            .ToListAsync(cancellationToken);

    protected override async Task AppendToStreamInternalAsync(IReadOnlyCollection<DomainEvent> events, CancellationToken cancellationToken = default)
    {
        if (events is null)
            throw new ArgumentNullException(nameof(events));

        var eventSchemas = events.Select(domainEvent => new Schema
        (
            domainEvent.AggregateRootId,
            domainEvent.Version,
            domainEvent.GetType().FullName!,
            domainEvent
        ));

        await _mongoClient
            .GetDatabase(_mongoSettings.DatabaseName)
            .GetCollection<Schema>(_mongoSettings.StreamName)
            .InsertManyAsync(eventSchemas, cancellationToken: cancellationToken);
    }

    protected override bool IsConcurrencyException(Exception ex)
    {
        throw new NotImplementedException();
    }

    private sealed record Schema
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