INSERT INTO [EventStreams].[EventStreams]
    ([StreamId]
    ,[StreamSequenceNumber]
    ,[EventType]
    ,[EventData])
 VALUES
    (@StreamId
    ,@StreamSequenceNumber
    ,@EventType
    ,@EventData)