using System.Data;
using System.Data.SqlClient;
using EzDomain.EventSourcing.EventStores.Sql;

namespace EzDomain.EventSourcing.EventStores.SqlServer;

public sealed class SqlServerConnectionFactory
    : SqlConnectionFactory
{
    public SqlServerConnectionFactory(string connectionString)
        : base(connectionString)
    {
    }

    public override IDbConnection CreateConnection() =>
        new SqlConnection(ConnectionString);
}