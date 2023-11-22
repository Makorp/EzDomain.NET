using EzDomain.EventSourcing.Serialization;
using EzDomain.EventSourcing.Tests.UnitTests.TestDoubles;

namespace EzDomain.EventSourcing.Tests.UnitTests.Serialization;

public abstract class SerializerTest<TEventDataSerializationType>
{
    private const string StringProp1Value = "123";
    private const string StringProp2Value = "456";

    private readonly IDomainEventDataSerializer<TEventDataSerializationType> _eventSerializer;

    protected SerializerTest(IDomainEventDataSerializer<TEventDataSerializationType> eventSerializer)
    {
        _eventSerializer = eventSerializer;
    }

    protected void TestSerializer()
    {
        // Arrange
        var domainEvent = new SerializationTestEvent(
            Guid.NewGuid().ToString(),
            StringProp1Value,
            StringProp2Value,
            123,
            new List<string> { "1", "2", "3" },
            new List<TestEventObject>
            {
                new("007"),
                new("005")
            });

        // Act
        var serializedEvent = _eventSerializer.Serialize(domainEvent);

        var deserializedEvent = (SerializationTestEvent)_eventSerializer.Deserialize(serializedEvent, domainEvent.GetType().AssemblyQualifiedName!)!;

        // Assert
        deserializedEvent.AggregateRootId
            .Should()
            .Be(domainEvent.AggregateRootId);

        deserializedEvent.Version
            .Should()
            .Be(domainEvent.Version);

        deserializedEvent.IntProp
            .Should()
            .Be(domainEvent.IntProp);

        deserializedEvent.StringProp
            .Should()
            .Be(domainEvent.StringProp);

        deserializedEvent.StringProp1
            .Should()
            .Be(domainEvent.StringProp1);

        deserializedEvent.ObjCollection
            .Should()
            .HaveCount(domainEvent.ObjCollection.Count);

        deserializedEvent.ObjCollection
            .Should()
            .BeEquivalentTo(domainEvent.ObjCollection);

        deserializedEvent.StringCollection
            .Should()
            .HaveCount(domainEvent.StringCollection.Count);

        deserializedEvent.StringCollection
            .Should()
            .BeEquivalentTo(domainEvent.StringCollection);
    }
}