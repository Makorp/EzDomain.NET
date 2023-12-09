using System.Reflection;

namespace EzDomain.EventStores.MongoDb.Tests.TestDoubles;

[Serializable]
internal sealed record TestEvent
    : DomainEvent
{
    /// <inheritdoc />
    public TestEvent(string aggregateRootId, string stringValue, int intValue, bool boolValue, DateTime dateTimeValue)
        : base(aggregateRootId)
    {
        StringValue = stringValue;
        IntValue = intValue;
        BoolValue = boolValue;
        DateTimeValue = dateTimeValue;
    }

    public string StringValue { get; }
    public int IntValue { get; }
    public bool BoolValue { get; }
    public DateTime DateTimeValue { get; }

    public void SetVersion(long version)
    {
        var versionField = typeof(DomainEvent).GetField("_version", BindingFlags.Instance | BindingFlags.NonPublic);
        if (versionField != null)
        {
            versionField.SetValue(this, version);
        }
    }
}