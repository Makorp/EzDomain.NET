namespace EzDomain.EventSourcing.Exceptions;

[ExcludeFromCodeCoverage]
[Serializable]
public class ConcurrencyException
    : Exception
{
    public ConcurrencyException()
    {
    }

    public ConcurrencyException(string message)
        : base(message)
    {
    }

    public ConcurrencyException(Exception innerException)
        : base("A concurrency exception occured while saving event stream to event store.", innerException)
    {
    }
    public ConcurrencyException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    protected ConcurrencyException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}