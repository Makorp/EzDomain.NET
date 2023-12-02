using System.Data;
using System.Diagnostics.CodeAnalysis;

namespace EzDomain.EventSourcing.EventStores.Sql;

public interface ISqlConnectionFactory
{
    IDbConnection CreateConnection();
}

[ExcludeFromCodeCoverage]
public abstract class SqlConnectionFactory
    : ISqlConnectionFactory
{
    protected SqlConnectionFactory(string connectionString) =>
        ConnectionString = connectionString;

    protected string ConnectionString { get; }

    public abstract IDbConnection CreateConnection();
}