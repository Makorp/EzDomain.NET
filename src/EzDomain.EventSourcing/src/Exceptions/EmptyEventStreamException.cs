namespace EzDomain.EventSourcing.Exceptions;

[ExcludeFromCodeCoverage]
[Serializable]
public class EmptyEventStreamException
    : Exception
{
    public EmptyEventStreamException(string message)
        : base(message)
    {
    }
}