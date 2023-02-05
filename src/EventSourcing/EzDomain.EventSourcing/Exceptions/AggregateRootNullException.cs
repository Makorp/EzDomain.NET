namespace EzDomain.EventSourcing.Exceptions;

[ExcludeFromCodeCoverage]
[Serializable]
public class AggregateRootNullException
    : Exception
{
    public AggregateRootNullException()
    {
    }

    public AggregateRootNullException(string message)
        : base(message)
    {
    }

    public AggregateRootNullException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    protected AggregateRootNullException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}