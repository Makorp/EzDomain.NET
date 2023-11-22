namespace EzDomain.EventSourcing.Exceptions;

[ExcludeFromCodeCoverage]
[Serializable]
public class EventStreamNullException
    : Exception
{
    public EventStreamNullException(string message)
        : base(message)
    {
    }
}