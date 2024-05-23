namespace EzDomain.EventStores.MongoDb;

public sealed record MongoEventStoreSettings(string DatabaseName, string CollectionName);