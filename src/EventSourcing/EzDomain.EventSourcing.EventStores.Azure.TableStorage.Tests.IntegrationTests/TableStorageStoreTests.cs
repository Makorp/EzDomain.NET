using AutoFixture.NUnit3;
using EzDomain.EventSourcing.Domain.Model;
using EzDomain.EventSourcing.EventStores.Azure.TableStorage.Tests.IntegrationTests.TestDoubles;
using EzDomain.EventSourcing.Serialization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EzDomain.EventSourcing.EventStores.Azure.TableStorage.Tests.IntegrationTests;

[TestFixture]
internal sealed class TableStorageStoreTests
{
    private readonly IEventDataSerializer<string> _eventDataSerializer;
    private readonly TableServiceClient _tableServiceClient;

    public TableStorageStoreTests()
    {
        var config = new ConfigurationBuilder()
            .AddEnvironmentVariables()
            .Build();

        _eventDataSerializer = new JsonEventDataSerializer();
        _tableServiceClient = new TableServiceClient(config["EzDomain:AzureTableStorage:ConnectionString"]);
    }

    private readonly Mock<ILogger> _mockLogger = new();

    [Test]
    [AutoData]
    public async Task GetByAggregateRootIdAsync_ReturnsAggregateRootEvents_WhenAggregateRootAndVersionAreProvided(string aggregateRootId, string stringValue, int intValue)
    {
        // Arrange
        var eventStore = new TableStorageStore(
            _mockLogger.Object,
            _eventDataSerializer,
            _tableServiceClient);

        var domainEventsToStore = new DomainEvent[]
        {
            new StringEvent(aggregateRootId, stringValue),
            new IntEvent(aggregateRootId, intValue)
        };

        long aggregateRootVersion = -1;

        foreach (var domainEvent in domainEventsToStore)
        {
            domainEvent.IncrementVersion(ref aggregateRootVersion);
        }

        // Act
        await eventStore.SaveAsync(domainEventsToStore, CancellationToken.None);
        await eventStore.SaveAsync(domainEventsToStore, CancellationToken.None);

        var events = await eventStore.GetByAggregateRootIdAsync(aggregateRootId, Constants.InitialVersion, CancellationToken.None);

        // Assert
    }
}