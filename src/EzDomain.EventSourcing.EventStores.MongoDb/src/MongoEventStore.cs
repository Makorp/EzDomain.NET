using EzDomain.EventSourcing.Domain.EventStores;
using EzDomain.EventSourcing.Domain.Model;
using Microsoft.Extensions.Logging;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;

namespace EzDomain.EventSourcing.EventStores.MongoDb;

public sealed class MongoEventStore
    : EventStore
{
    private readonly IMongoClient _mongoClient;
    private readonly MongoEventStoreSettings _mongoSettings;

    static MongoEventStore()
    {
        if(BsonClassMap.IsClassMapRegistered(typeof(DomainEvent)))
           return;

        BsonClassMap.RegisterClassMap<DomainEvent>(classMap =>
        {
            classMap.SetIsRootClass(true);
            classMap.AutoMap();
            classMap.MapField("_version").SetElementName("Version");
        });
    }

    public MongoEventStore(ILogger logger, IMongoClient mongoClient, MongoEventStoreSettings mongoSettings)
        : base(logger)
    {
        _mongoClient = mongoClient;
        _mongoSettings = mongoSettings;
    }

    public override async Task<IReadOnlyCollection<DomainEvent>> GetEventStreamAsync(string streamId, long fromVersion, CancellationToken cancellationToken = default)
    {
        var collection = _mongoClient
            .GetDatabase(_mongoSettings.DatabaseName)
            .GetCollection<DomainEventSchema>(_mongoSettings.CollectionName);
    
        var filtersBuilder = Builders<DomainEventSchema>.Filter;
        
        var filters = filtersBuilder.And(
            filtersBuilder.Eq(x=> x.Id.StreamId, streamId),
            filtersBuilder.Gte(x=> x.Id.StreamSequenceNumber, fromVersion)
        );

        var domainEvents = await collection.FindAsync<DomainEventSchema>(filters, cancellationToken: cancellationToken);

        return domainEvents
            .ToList(cancellationToken)
            .Select(eventDocument => eventDocument.EventData)
            .ToList();
    }

    protected override async Task AppendToStreamInternalAsync(IReadOnlyCollection<DomainEvent> domainEvents, CancellationToken cancellationToken = default)
    {
        var eventSchemas = domainEvents.Select(domainEvent => new DomainEventSchema
        {
            Id = new DomainEventSchemaId
            {
                StreamId = domainEvent.AggregateRootId,
                StreamSequenceNumber = domainEvent.Version,
            },
            EventData = domainEvent
        });
        
        await _mongoClient
            .GetDatabase(_mongoSettings.DatabaseName)
            .GetCollection<DomainEventSchema>(_mongoSettings.CollectionName)
            .InsertManyAsync(eventSchemas, cancellationToken: cancellationToken);
    }

    protected override bool IsConcurrencyException(Exception ex) =>
        ex is MongoBulkWriteException<DomainEventSchema> mongoBulkWriteException &&
        mongoBulkWriteException.WriteErrors.Any(writeError => writeError.Code == 11000);

    internal sealed class DomainEventSchema
    {
        [BsonId]
        public DomainEventSchemaId Id { get; set; }

        public DomainEvent EventData { get; set; }
    }

    internal sealed class DomainEventSchemaId
    {
        public string StreamId { get; set; }

        public long StreamSequenceNumber { get; set; }
    }
}