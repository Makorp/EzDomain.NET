SELECT
    [es].[EventType],
    [es].[EventData]
FROM [dbo].[EventStreams] [es]
WHERE [es].[StreamId] = @StreamId
AND [es].[StreamSequenceNumber] >= @FromVersion