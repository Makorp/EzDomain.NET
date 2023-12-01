namespace EzDomain.EventSourcing.EventStores.MongoDb;

public sealed record MongoEventStoreSettings(string DatabaseName, string CollectionName, IReadOnlyCollection<Type> KnownTypes);