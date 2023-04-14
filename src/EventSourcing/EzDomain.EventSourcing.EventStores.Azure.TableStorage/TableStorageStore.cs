using Azure;
using Azure.Data.Tables;
using EzDomain.EventSourcing.Domain.EventStores;
using EzDomain.EventSourcing.Domain.Model;
using EzDomain.EventSourcing.Serialization;
using Microsoft.Extensions.Logging;

namespace EzDomain.EventSourcing.EventStores.Azure.TableStorage;

public sealed class TableStorageStore
    : EventStore
{
    private const string DefaultEventStoreName = "EventStore";

    private readonly IEventDataSerializer<string> _serializer;
    private readonly TableServiceClient _tableServiceClient;

    public TableStorageStore(ILogger logger, IEventDataSerializer<string> serializer, TableServiceClient tableServiceClient)
        : base(logger)
    {
        _serializer = serializer;
        _tableServiceClient = tableServiceClient;
    }

    public override async Task<IReadOnlyCollection<DomainEvent>> GetByAggregateRootIdAsync(string aggregateRootId, long fromVersion, CancellationToken cancellationToken = default)
    {
        var tableClient = _tableServiceClient.GetTableClient(DefaultEventStoreName);

        var tableEntities = await tableClient
            .QueryAsync<TableEntity>(
                tableEntity =>
                    tableEntity.PartitionKey.Equals(aggregateRootId) &&
                    tableEntity.GetInt64(nameof(tableEntity.RowKey)) >= fromVersion,
                cancellationToken: cancellationToken)
            .ToListAsync(cancellationToken);

        var domainEvents = tableEntities
            .Select(ParseToEvent)
            .OrderBy(domainEvent => domainEvent.Version)
            .ToList();

        return domainEvents;
    }

    protected override async Task SaveInternalAsync(IReadOnlyCollection<DomainEvent> events, CancellationToken cancellationToken = default)
    {
        var tableClient = _tableServiceClient.GetTableClient(DefaultEventStoreName);

        var streamId = Guid.NewGuid().ToString();

        var transactionActions = events
            .Select(domainEvent => CreateTableTransactionAction(domainEvent, streamId))
            .ToList();

        await tableClient.SubmitTransactionAsync(transactionActions, cancellationToken);
    }

    protected override bool IsConcurrencyException(Exception ex) => ex is TableTransactionFailedException
    {
        ErrorCode: "EntityAlreadyExists",
        Status: 409
    };

    private DomainEvent ParseToEvent(TableEntity tableEntity)
    {
        var eventData = tableEntity.GetString("Data");
        var eventType = tableEntity.GetString("Type");

        var domainEvent = _serializer.Deserialize(eventData, eventType);

        return domainEvent;
    }

    private TableTransactionAction CreateTableTransactionAction(DomainEvent domainEvent, string streamId) =>
        new(
            TableTransactionActionType.Add,
            ParseToTableEntity(domainEvent, streamId),
            new ETag($"{domainEvent.AggregateRootId}_{domainEvent.Version}"));

    private TableEntity ParseToTableEntity(DomainEvent domainEvent, string streamId) =>
        new(domainEvent.AggregateRootId, domainEvent.Version.ToString())
        {
            { "StreamId", streamId },
            { "Type", domainEvent.GetType().AssemblyQualifiedName },
            { "Data", _serializer.Serialize(domainEvent) }
        };
}