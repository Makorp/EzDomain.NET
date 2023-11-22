namespace EzDomain.EventSourcing.Exceptions;

[ExcludeFromCodeCoverage]
[Serializable]
public class AggregateRootIdException
    : Exception
{
    public AggregateRootIdException(string message)
        : base(message)
    {
    }
}