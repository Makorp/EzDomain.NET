namespace EzDomain.EventSourcing.EventStores.MongoDb;

public sealed record MongoEventStoreSettings(string DatabaseName, string StreamName);