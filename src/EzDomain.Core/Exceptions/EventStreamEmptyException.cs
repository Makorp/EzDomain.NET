namespace EzDomain.Core.Exceptions;

[ExcludeFromCodeCoverage]
[Serializable]
public class EventStreamEmptyException
    : Exception
{
    public EventStreamEmptyException(string message)
        : base(message)
    {
    }
}