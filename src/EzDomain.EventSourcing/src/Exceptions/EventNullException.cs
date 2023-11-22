namespace EzDomain.EventSourcing.Exceptions;

[ExcludeFromCodeCoverage]
[Serializable]
public class EventNullException
    : Exception
{
    public EventNullException(string message)
        : base(message)
    {
    }
}