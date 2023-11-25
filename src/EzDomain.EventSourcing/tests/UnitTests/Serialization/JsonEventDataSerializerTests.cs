using EzDomain.EventSourcing.Serialization;
using EzDomain.EventSourcing.Tests.UnitTests.TestDoubles;

namespace EzDomain.EventSourcing.Tests.UnitTests.Serialization;

[TestFixture]
public sealed class JsonEventDataSerializerTests
    : SerializerTest<string>
{
    public JsonEventDataSerializerTests()
        : base(new JsonDomainEventSerializer())
    {
    }

    [Test]
    public void SerializeAndDeserialize_CreatesEqualEvents_WhenDomainEventIsGiven()
        => TestSerializer();

    
    [TestCase(null)]
    [TestCase("")]
    [TestCase(" ")]
    public void Serialize_ThrowsArgumentNullException_WhenDomainEventIsNull(string domainEventTypeFullName)
    {
        // Arrange
        var jsonDomainEventSerializer = new JsonDomainEventSerializer();

        // Act
        Action act = () => jsonDomainEventSerializer.Serialize(null);

        // Assert
        act
            .Should()
            .Throw<ArgumentNullException>();
    }
    [TestCase(null)]
    [TestCase("")]
    [TestCase(" ")]
    public void Deserialize_ThrowsArgumentNullException_WhenSerializedEventDataIsNullOrWhitespace(string jsonString)
    {
        // Arrange
        var jsonDomainEventSerializer = new JsonDomainEventSerializer();

        // Act
        Action act = () => jsonDomainEventSerializer.Deserialize(jsonString, typeof(BehaviorExecuted).FullName);

        // Assert
        act
            .Should()
            .Throw<ArgumentNullException>();
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase(" ")]
    public void Deserialize_ThrowsArgumentNullException_WhenDomainEventTypeFullNameIsNullOrWhitespace(string domainEventTypeFullName)
    {
        // Arrange
        var jsonDomainEventSerializer = new JsonDomainEventSerializer();

        // Act
        Action act = () => jsonDomainEventSerializer.Deserialize("{}", domainEventTypeFullName);

        // Assert
        act
            .Should()
            .Throw<ArgumentNullException>();
    }
}