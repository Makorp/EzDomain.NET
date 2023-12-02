INSERT INTO [dbo].[EventStreams]
    ([StreamId]
    ,[StreamSequenceNumber]
    ,[EventType]
    ,[EventData])
 VALUES
    (@StreamId
    ,@StreamSequenceNumber
    ,@EventType
    ,@EventData)