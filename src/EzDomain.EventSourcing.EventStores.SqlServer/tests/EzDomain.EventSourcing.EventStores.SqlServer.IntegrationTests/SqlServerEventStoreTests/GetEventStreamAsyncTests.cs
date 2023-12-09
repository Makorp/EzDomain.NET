using System.Data;
using System.Data.SqlClient;
using Dapper;
using EzDomain.EventSourcing.EventStores.Sql;
using EzDomain.EventSourcing.Serialization;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace EzDomain.EventSourcing.EventStores.SqlServer.IntegrationTests.SqlServerEventStoreTests;

[TestFixture]
internal sealed class GetEventStreamAsyncTests
{
    private const string DefaultConnectionString = "Server=localhost;User Id=sa;Password=Admin123"; // TODO: Take this connectionString from user secrets

    private const string CreateDbSchemaScript = @"
        IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'EventStreams')
        BEGIN
            EXEC ('CREATE SCHEMA [EventStreams] AUTHORIZATION [dbo]')
        END";

    private const string CreateTableScript = @"
        CREATE TABLE [EventStreams].[DomainEvents]
        (
            [StreamId] NVARCHAR(50) NOT NULL,
            [StreamSequenceNumber] BIGINT NOT NULL,
            [EventType] NVARCHAR(50) NOT NULL,
            [EventData] NVARCHAR(MAX) NOT NULL,
            CONSTRAINT [PK_DomainEvents] PRIMARY KEY CLUSTERED ([StreamId] ASC, [StreamSequenceNumber] ASC)
        );";

    private static readonly string TestDatabaseName = $"TestEventStore_{Guid.NewGuid().ToString()}";
    private static readonly string CreateDbScript = $"CREATE DATABASE [{TestDatabaseName}]";
    private static readonly string DropDbScript = $"DROP DATABASE [{TestDatabaseName}]";

    private readonly ILogger _logger = NullLogger.Instance;
    private readonly ISqlConnectionFactory _connectionFactory = new SqlServerConnectionFactory($"Server=localhost;Database={TestDatabaseName};User Id=sa;Password=Admin123"); // TODO: Take this connectionString from user secrets
    private readonly ISqlScriptsLoader _scriptsLoader = new SqlScriptsLoader(typeof(SqlServerEventStore).Assembly);
    private readonly IDomainEventSerializer<string> _domainEventSerializer = new JsonDomainEventSerializer();

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        using var connection = new SqlConnection(DefaultConnectionString);
        if (connection.State != ConnectionState.Open)
            connection.Open();

        connection.Execute(CreateDbScript);
        connection.ChangeDatabase(TestDatabaseName);
        connection.Execute(CreateDbSchemaScript);
        connection.Execute(CreateTableScript);
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        using var connection = new SqlConnection(DefaultConnectionString);
        connection.Execute(DropDbScript);
    }

    [Test]
    public async Task GetEventStreamAsyncReturnsEmptyStream_WhenStreamDoesNotExist()
    {
        // Arrange
        var streamId = Guid.NewGuid().ToString();

        var sut = new SqlServerEventStore(
            _logger,
            _connectionFactory,
            _scriptsLoader,
            _domainEventSerializer);

        // Act
        var stream = await sut.GetEventStreamAsync(streamId, 0);

        // Assert
        stream
            .Should()
            .NotBeNull();

        stream
            .Should()
            .BeEmpty();
    }

    [Test]
    public async Task GetEvenStreamAsyncReturnsDomainEvents_WhenStreamExist()
    {
        // Arrange
        var streamId = Guid.NewGuid().ToString();

        var sut = new SqlServerEventStore(
            _logger,
            _connectionFactory,
            _scriptsLoader,
            _domainEventSerializer);

        // Assert
        

        // Act
        
    }
}