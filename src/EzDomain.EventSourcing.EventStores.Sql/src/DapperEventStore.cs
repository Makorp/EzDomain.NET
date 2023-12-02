using System.Diagnostics.CodeAnalysis;
using Dapper;
using EzDomain.EventSourcing.Domain.EventStores;
using EzDomain.EventSourcing.Domain.Model;
using EzDomain.EventSourcing.Serialization;
using Microsoft.Extensions.Logging;

namespace EzDomain.EventSourcing.EventStores.Sql;

[ExcludeFromCodeCoverage]
public abstract class DapperEventStore
    : EventStore
{
    protected DapperEventStore(ILogger logger, ISqlConnectionFactory connectionFactory, ISqlScriptsLoader scriptsLoader, IDomainEventSerializer<string> domainEventSerializer)
        : base(logger)
    {
        ConnectionFactory = connectionFactory;
        ScriptsLoader = scriptsLoader;
        DomainEventSerializer = domainEventSerializer;
    }

    public ISqlConnectionFactory ConnectionFactory { get; }

    public ISqlScriptsLoader ScriptsLoader { get; }

    public IDomainEventSerializer<string> DomainEventSerializer { get; }

    public override async Task<IReadOnlyCollection<DomainEvent>> GetEventStreamAsync(string streamId, long fromVersion, CancellationToken cancellationToken = default)
    {
        var sql = ScriptsLoader.GetScript("GetEventStream");

        var parameters = new
        {
            StreamId = streamId,
            FromVersion = fromVersion
        };

        var commandDefinition = new CommandDefinition(sql, parameters, cancellationToken: cancellationToken);

        using var connection = ConnectionFactory.CreateConnection();

        var domainEventsData = await connection.QueryAsync<DomainEventData>(commandDefinition);

        var domainEvents = domainEventsData
            .Select(x => DomainEventSerializer.Deserialize(x.EventData, x.EventType)!)
            .ToList();

        return domainEvents;
    }

    protected override async Task AppendToStreamInternalAsync(IReadOnlyCollection<DomainEvent> domainEvents, CancellationToken cancellationToken = default)
    {
        var sql = ScriptsLoader.GetScript("AppendToStream");

        using var connection = ConnectionFactory.CreateConnection();
        using var transaction = connection.BeginTransaction();
        
        foreach (var domainEvent in domainEvents)
        {
            var parameters = new
            {
                StreamId = domainEvent.AggregateRootId,
                FromVersion = domainEvent.Version,
                EventType = domainEvent.GetType().AssemblyQualifiedName,
                EventData = DomainEventSerializer.Serialize(domainEvent)
            };
            
            var commandDefinition = new CommandDefinition(sql, parameters, transaction, cancellationToken: cancellationToken);

            await connection.ExecuteAsync(commandDefinition);
        }

        transaction.Commit();
    }

    internal sealed record DomainEventData(string EventType, string EventData);
}