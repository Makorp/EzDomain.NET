using EzDomain.EventSourcing.Domain.EventStores;
using EzDomain.EventSourcing.EventStores.Sql;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace EzDomain.EventSourcing.EventStores.SqlServer.DependencyInjection;

public static class SqlServerEventStoreExt
{
    private const string ConnectionStringName = "SqlServerConnectionString";

    public static IServiceCollection AddSqlServerEventStore(this IServiceCollection services)
    {
        services.TryAddSingleton(typeof(ISqlScriptsLoader), _ =>
            new SqlScriptsLoader(typeof(SqlServerEventStore).Assembly));

        services.TryAddSingleton(typeof(ISqlConnectionFactory), serviceProvider =>
        {
            var configuration = serviceProvider.GetRequiredService<IConfiguration>();

            var connectionString = configuration.GetConnectionString(ConnectionStringName);
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new InvalidOperationException($"{ConnectionStringName} is not configured.");

            return new SqlServerConnectionFactory(connectionString);
        });

        services.TryAddSingleton<IEventStore, SqlServerEventStore>();

        return services;
    }
}