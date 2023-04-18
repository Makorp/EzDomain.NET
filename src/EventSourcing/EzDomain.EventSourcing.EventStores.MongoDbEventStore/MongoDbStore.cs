using EzDomain.EventSourcing.Domain.EventStores;
using EzDomain.EventSourcing.Domain.Model;
using Microsoft.Extensions.Logging;
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

    public override async Task<IReadOnlyCollection<DomainEvent>> GetByAggregateRootIdAsync(string aggregateRootId, long fromVersion, CancellationToken cancellationToken = default) =>
        await _mongoClient
            .GetDatabase("EventStore")
            .GetCollection<DomainEvent>("Events")
            .Find(domainEvent =>
                domainEvent.AggregateRootId == aggregateRootId &&
                domainEvent.Version >= fromVersion)
            .ToListAsync(cancellationToken);

    protected override async Task SaveInternalAsync(IReadOnlyCollection<DomainEvent> events, CancellationToken cancellationToken = default)
    {
        await _mongoClient
            .GetDatabase("EventStore")
            .GetCollection<DomainEvent>("Events")
            .InsertManyAsync(events, cancellationToken: cancellationToken);
    }

    protected override bool IsConcurrencyException(Exception ex)
    {
        throw new NotImplementedException();
    }
}