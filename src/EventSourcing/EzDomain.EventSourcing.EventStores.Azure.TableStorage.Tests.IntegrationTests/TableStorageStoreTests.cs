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
    private readonly IConfiguration _config = new ConfigurationBuilder()
        .AddEnvironmentVariables()
        .Build();

    private readonly Mock<ILogger> _mockLogger = new();

    [Test]
    [AutoData]
    public async Task GetByAggregateRootIdAsync_ReturnsAggregateRootEvents_WhenAggregateRootAndVersionAreProvided(string aggregateRootId, string stringValue, int intValue)
    {
        // Arrange
        var eventStore = new TableStorageStore(
            _mockLogger.Object,
            new JsonEventDataSerializer(),
            new TableServiceClient(_config["AzureTableStorage:ConnectionString"]));

        var eventsToStore = new DomainEvent[]
        {
            new StringEvent(aggregateRootId, stringValue),
            new IntEvent(aggregateRootId, intValue)
        };

        long aggregateRootVersion = -1;

        foreach (var @event in eventsToStore)
        {
            @event.IncrementVersion(ref aggregateRootVersion);
        }

        // Act
        await eventStore.SaveAsync(eventsToStore, CancellationToken.None);
        await eventStore.SaveAsync(eventsToStore, CancellationToken.None);

        var events = await eventStore.GetByAggregateRootIdAsync(aggregateRootId, -1, CancellationToken.None);

        // Assert
    }
}