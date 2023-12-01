﻿using EzDomain.EventSourcing.EventStores.MongoDb.IntegrationTests.TestDoubles;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using Moq;

namespace EzDomain.EventSourcing.EventStores.MongoDb.IntegrationTests.MongoEventStoreTests;

internal abstract class TestsBase
{
    protected static readonly MongoEventStoreSettings MongoEventStoreSettings = new(
        $"testEventStore_{Guid.NewGuid().ToString()}",
        $"domainEvents_{Guid.NewGuid().ToString()}",
        new[]
        {
            typeof(TestEvent)
        });

    protected static readonly MongoClient MongoClient = new(GetConnectionString());

    protected static readonly MongoEventStore SystemUnderTest;

    private static readonly Mock<ILogger> MockLogger = new();

    static TestsBase()
    {
        SystemUnderTest = new MongoEventStore(
            MockLogger.Object,
            MongoClient,
            MongoEventStoreSettings);
    }

    [OneTimeSetUp]
    public virtual void OneTimeSetUp() =>
        MongoClient
            .GetDatabase(MongoEventStoreSettings.DatabaseName)
            .GetCollection<BsonDocument>(MongoEventStoreSettings.CollectionName);

    [OneTimeTearDown]
    public virtual void OneTimeTearDown() =>
        MongoClient
            .DropDatabase(MongoEventStoreSettings.DatabaseName);

    [SetUp]
    public virtual void SetUp() =>
        MongoClient
            .GetDatabase(MongoEventStoreSettings.DatabaseName)
            .GetCollection<BsonDocument>(MongoEventStoreSettings.CollectionName);

    [TearDown]
    public void TearDown() =>
        MongoClient
            .GetDatabase(MongoEventStoreSettings.DatabaseName)
            .DropCollection(MongoEventStoreSettings.CollectionName);

    private static string GetConnectionString()
    {
        var configuration = new ConfigurationBuilder()
            .AddUserSecrets(typeof(TestsBase).Assembly)
            .Build();

        return configuration["MongoDb:ConnectionString"]!;
    }
}