namespace EzDomain.EventSourcing.Exceptions;

[ExcludeFromCodeCoverage]
[Serializable]
public class AggregateRootVersionException
    : Exception
{

    public AggregateRootVersionException(string message)
        : base(message)
    {
    }
}