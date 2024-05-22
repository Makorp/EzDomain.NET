SELECT
    [de].[EventType],
    [de].[EventData]
FROM [EventStreams].[DomainEvents] [de]
WHERE [de].[StreamId] = @StreamId
AND [de].[StreamSequenceNumber] >= @FromVersion