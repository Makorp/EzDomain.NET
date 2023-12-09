using System.Data.SqlClient;
using EzDomain.EventSourcing.EventStores.Sql;
using EzDomain.EventSourcing.Serialization;
using Microsoft.Extensions.Logging;

namespace EzDomain.EventSourcing.EventStores.SqlServer;

public sealed class SqlServerEventStore
    : DapperEventStore
{
    public SqlServerEventStore(ILogger logger, ISqlConnectionFactory connectionFactory, ISqlScriptsLoader scriptsLoader, IDomainEventSerializer<string> domainEventSerializer)
        : base(logger, connectionFactory, scriptsLoader, domainEventSerializer)
    {
    }

    protected override bool IsConcurrencyException(Exception ex) =>
        ex is SqlException { ErrorCode: 2627 };
}