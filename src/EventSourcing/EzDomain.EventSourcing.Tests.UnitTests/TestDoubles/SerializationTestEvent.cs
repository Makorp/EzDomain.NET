using EzDomain.EventSourcing.Domain.Model;

namespace EzDomain.EventSourcing.Tests.UnitTests.TestDoubles;

internal sealed record SerializationTestEvent(string AggregateRootId, string StringProp, string StringProp1,
        int IntProp, IReadOnlyCollection<string> StringCollection, IReadOnlyCollection<TestEventObject> ObjCollection)
    : DomainEvent(AggregateRootId);