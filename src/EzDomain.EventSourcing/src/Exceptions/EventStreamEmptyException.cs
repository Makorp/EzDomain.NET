namespace EzDomain.EventSourcing.Exceptions;

[ExcludeFromCodeCoverage]
[Serializable]
public class EventStreamEmptyException
    : Exception
{
    public EventStreamEmptyException(string message)
        : base(message)
    {
    }
}