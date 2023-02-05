namespace EzDomain.EventSourcing.Exceptions;

[ExcludeFromCodeCoverage]
[Serializable]
public class AggregateRootIdException
    : Exception
{
    public AggregateRootIdException()
    {
    }

    public AggregateRootIdException(string message)
        : base(message)
    {
    }

    public AggregateRootIdException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    protected AggregateRootIdException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}