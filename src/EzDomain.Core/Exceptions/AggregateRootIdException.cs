namespace EzDomain.Core.Exceptions;

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