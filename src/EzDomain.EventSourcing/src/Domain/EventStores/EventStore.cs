using EzDomain.EventSourcing.Domain.Model;
using EzDomain.EventSourcing.Exceptions;
using Microsoft.Extensions.Logging;

namespace EzDomain.EventSourcing.Domain.EventStores;

[ExcludeFromCodeCoverage]
public abstract class EventStore
    : IEventStore
{
    private readonly ILogger _logger;

    protected EventStore(ILogger logger) => _logger = logger;

    public abstract Task<IReadOnlyCollection<DomainEvent>> GetEventStreamAsync(string streamId, long fromVersion, CancellationToken cancellationToken = default);

    public virtual async Task AppendToStreamAsync(IReadOnlyCollection<DomainEvent> events, CancellationToken cancellationToken = default)
    {
        try
        {
            await AppendToStreamInternalAsync(events, cancellationToken);
        }
        catch (Exception ex)
        {
            if (IsConcurrencyException(ex))
            {
                var concurrencyException = new ConcurrencyException("A concurrency exception occured while saving event stream to the event store.", ex);

                _logger.LogError(concurrencyException, concurrencyException.Message);

                throw concurrencyException;
            }

            _logger.LogError(ex, ex.Message);

            throw;
        }
    }

    protected abstract Task AppendToStreamInternalAsync(IReadOnlyCollection<DomainEvent> events, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if concurrency occured while saving domain events to event store.
    /// </summary>
    /// <param name="ex">Exception that occured in event store.</param>
    /// <returns>Returns true if exception was caused by domain events concurrency.</returns>
    protected abstract bool IsConcurrencyException(Exception ex);
}