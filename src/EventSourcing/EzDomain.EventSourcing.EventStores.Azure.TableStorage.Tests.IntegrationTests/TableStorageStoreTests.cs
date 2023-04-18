using AutoFixture.NUnit3;
using EzDomain.EventSourcing.Domain.Model;
using EzDomain.EventSourcing.EventStores.Azure.TableStorage.Tests.IntegrationTests.TestDoubles;
using EzDomain.EventSourcing.Exceptions;
using EzDomain.EventSourcing.Serialization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EzDomain.EventSourcing.EventStores.Azure.TableStorage.Tests.IntegrationTests;

[TestFixture]
internal sealed class TableStorageStoreTests
{
    private readonly string _azureTableStorageConnectionString;

    private readonly Mock<ILogger> _mockLogger = new();

    public TableStorageStoreTests()
    {
        var config = new ConfigurationBuilder()
            .AddUserSecrets(typeof(TableStorageStoreTests).Assembly)
            .Build();

        _azureTableStorageConnectionString = config["AzureTableStorage:ConnectionString"]!;
    }

    [Test]
    [AutoData]
    public async Task SaveAsync_ThrowsConcurrencyException_WhenEventsWithTheSameVersionsExist(string aggregateRootId, int intValue)
    {
        // Arrange
        var eventStore = new TableStorageStore(
            _mockLogger.Object,
            new JsonEventDataSerializer(),
            new TableServiceClient(_azureTableStorageConnectionString));

        var eventsToStore = new DomainEvent[]
        {
            new IntEvent(aggregateRootId, intValue)
        };

        // Act
        Func<Task> actSaveAsync = async () =>
        {
            await eventStore.AppendToStreamAsync(eventsToStore, CancellationToken.None);
            await eventStore.AppendToStreamAsync(eventsToStore, CancellationToken.None);
        };

        // Assert
        await actSaveAsync
            .Should()
            .ThrowExactlyAsync<ConcurrencyException>();
    }

    [Test]
    [AutoData]
    public async Task GetByAggregateRootIdAsync_ReturnsAggregateRootEvents_WhenSaveAsyncSavedEventsInEventStore(string aggregateRootId, string stringValue, int intValue)
    {
        // Arrange
        var eventStore = new TableStorageStore(
            _mockLogger.Object,
            new JsonEventDataSerializer(),
            new TableServiceClient(_azureTableStorageConnectionString));

        var eventsToStore = new DomainEvent[]
        {
            new StringEvent(aggregateRootId, stringValue),
            new IntEvent(aggregateRootId, intValue)
        };

        var aggregateRootVersion = Constants.InitialVersion;

        foreach (var @event in eventsToStore)
        {
            @event.IncrementVersion(ref aggregateRootVersion);
        }

        // Act
        await eventStore.AppendToStreamAsync(eventsToStore, CancellationToken.None);

        var events = await eventStore.GetEventStreamAsync(aggregateRootId, Constants.InitialVersion, CancellationToken.None);

        // Assert
        foreach (var domainEvent in events)
        {
            eventsToStore
                .Should()
                .Contain(domainEvent);
        }
    }
}