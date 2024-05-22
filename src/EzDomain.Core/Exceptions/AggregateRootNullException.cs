namespace EzDomain.Core.Exceptions;

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