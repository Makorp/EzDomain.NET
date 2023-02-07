using EzDomain.EventSourcing.Serialization;

namespace EzDomain.EventSourcing.Tests.UnitTests.Serialization;

[TestFixture]
public sealed class JsonEventDataSerializerTests
    : SerializerTest<string>
{
    public JsonEventDataSerializerTests()
        : base(new JsonEventDataSerializer())
    {
    }

    [Test]
    public void SerializeAndDeserialize_CreatesEqualEvents_WhenDomainEventIsGiven() => TestSerializer();
}