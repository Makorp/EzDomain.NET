namespace EzDomain.Core.Exceptions;

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