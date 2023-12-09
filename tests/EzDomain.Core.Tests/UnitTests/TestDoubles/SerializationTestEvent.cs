using EzDomain.Core.Domain.Model;

namespace EzDomain.Core.Tests.UnitTests.TestDoubles;

internal sealed record SerializationTestEvent(
        string AggregateRootId,
        string StringProp,
        string StringProp1,
        int IntProp,
        IReadOnlyCollection<string> StringCollection,
        IReadOnlyCollection<TestEventObject> ObjCollection)
    : DomainEvent(AggregateRootId);