namespace EzDomain.EventSourcing.Exceptions;

[ExcludeFromCodeCoverage]
[Serializable]
public class AggregateRootNullException
    : Exception
{
    public AggregateRootNullException(string message)
        : base(message)
    {
    }
}