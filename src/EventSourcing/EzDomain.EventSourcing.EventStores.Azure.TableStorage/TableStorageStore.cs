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
    private readonly IEventDataSerializer<string> _serializer;
    private readonly TableServiceClient _tableServiceClient;

    private readonly string _eventStoreName;

    public TableStorageStore(ILogger logger, IEventDataSerializer<string> serializer, TableServiceClient tableServiceClient, string eventStoreName = Constants.DefaultEventStoreName)
        : base(logger)
    {
        _serializer = serializer;
        _tableServiceClient = tableServiceClient;

        _eventStoreName = eventStoreName;
    }

    public override async Task<IReadOnlyCollection<DomainEvent>> GetEventStreamAsync(string aggregateRootId, long fromVersion, CancellationToken cancellationToken = default)
    {
        var tableClient = _tableServiceClient.GetTableClient(_eventStoreName);

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

    protected override async Task AppendToStreamInternalAsync(IEnumerable<DomainEvent> events, CancellationToken cancellationToken = default)
    {
        var tableClient = _tableServiceClient.GetTableClient(_eventStoreName);

        var transactionActions = events
            .Select(CreateTableTransactionAction)
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

    private TableTransactionAction CreateTableTransactionAction(DomainEvent domainEvent) =>
        new(
            TableTransactionActionType.Add,
            ParseToTableEntity(domainEvent),
            new ETag($"{domainEvent.AggregateRootId}_{domainEvent.Version}"));

    private TableEntity ParseToTableEntity(DomainEvent domainEvent) =>
        new(domainEvent.AggregateRootId, domainEvent.Version.ToString())
        {
            { "Type", domainEvent.GetType().AssemblyQualifiedName },
            { "Data", _serializer.Serialize(domainEvent) }
        };
}