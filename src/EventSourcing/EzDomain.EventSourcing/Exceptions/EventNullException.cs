namespace EzDomain.EventSourcing.Exceptions;

[ExcludeFromCodeCoverage]
[Serializable]
public class EventNullException
    : Exception
{
    public EventNullException()
    {
    }

    public EventNullException(string message)
        : base(message)
    {
    }

    public EventNullException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    protected EventNullException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}