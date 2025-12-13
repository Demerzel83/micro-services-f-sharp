namespace Microsoft.eShopOnContainers.Services.Catalog.Infrastructure.SqlServer.EventsDB

open Microsoft.eShopOnContainers.Services.Catalog.Core.Types
open System
open Newtonsoft.Json
open Microsoft.Data.SqlClient
open Dapper
open System.Data
open Microsoft.eShopOnContainers.Services.Catalog.Core.Generated
open Microsoft.eShopOnContainers.Services.Catalog.Core.Generated.Events.Types
open TaskResult

type DataAccessError =
    | DataAccessError of string
    interface IError

module private DataAccess =
    [<Literal>] 
    let connectionString = "Server=localhost;Database=Events;User=sa;Password=MyVeryStrongPassword123!;TrustServerCertificate=True"

    let entityToEvent<'TEvent when 'TEvent :> IEvent>(e: Events.Types.dbo.Event) =
        let pid =
            match e.ProcessId with
            | Some p -> Some (ProcessId p)
            | None -> None

        { EventNumber = e.Id
          EventId = EventId e.EventId
          AggregateId = AggregateId e.AggregateId
          CausationId = CausationId e.CausationId
          CorrelationId = CorrelationId e.CorrelationId
          ProcessId = pid
          Payload = JsonConvert.DeserializeObject(e.EventPayload, Type.GetType(e.PayloadType)) :?> 'TEvent}

    let entityToCommand<'TCommand when 'TCommand :> ICommand>(c: Events.Types.dbo.Command) =
        let pid =
            match c.ProcessId with
            | Some p -> Some(ProcessId p)
            | None -> None

        let version =
            match c.ExpectedVersion with
            | Some v -> Expected v
            | _ -> Irrelevant

        { ExpectedVersion = version
          CommandId = CommandId c.CommandId
          AggregateId = AggregateId c.AggregateId
          CausationId = CausationId c.CausationId
          CorrelationId = CorrelationId c.CorrelationId
          ProcessId = pid
          Payload = JsonConvert.DeserializeObject(c.CommandPayload, Type.GetType(c.PayloadType)) :?> 'TCommand }

    let processIdToGuidOption pid =
        match pid with
        | Some pid ->
            let (ProcessId id) = pid
            Some id
        | _ -> None

    let processIdToGuid pid =
        match pid with
        | Some pid ->
            let (ProcessId id) = pid
            id
        | _ -> Guid.Empty

    let eventToEntity<'TEvent when 'TEvent :> IEvent> (Category category) (e:EventEnvelope<'TEvent>) : TaskResult<dbo.Event, IError> =
        let typeName = e.Payload.GetType().AssemblyQualifiedName
        let (AggregateId aggId) = e.AggregateId
        let (EventId evId) = e.EventId
        let (CausationId causId) = e.CausationId
        let (CorrelationId corrId) = e.CorrelationId
        let payload = JsonConvert.SerializeObject(e.Payload)
        let newEvent: dbo.Event = {
            Id = 0
            EventId = evId
            AggregateId = aggId
            CausationId = causId
            CorrelationId = corrId
            ProcessId = processIdToGuidOption e.ProcessId
            EventPayload = payload
            PayloadType = typeName
            Category = category
        }

        let newEventParams = {|
            EventId = evId
            AggregateId = aggId
            CausationId = causId
            CorrelationId = corrId
            ProcessId = processIdToGuid e.ProcessId
            EventPayload = payload
            PayloadType = typeName
            Category = category
        |}
        
        try 
            task {
                let query = "INSERT INTO dbo.Event
                    ([EventId],[AggregateId],[CausationId],[CorrelationId],[ProcessId],[EventPayload],[PayloadType],[Category])
                VALUES (@EventId, @AggregateId, @CausationId, @CorrelationId, @ProcessId, @EventPayload, @PayloadType, @Category)"
                use connection = new SqlConnection(connectionString)
                let! _ = connection.ExecuteScalarAsync<int>(query, newEventParams)  
                return Ok newEvent
                }
        with ex -> Error ex.Message :> IError |> Result.Error |> MyTask.rtn
        

    let commandToEntity<'TCommand when 'TCommand:> ICommand> (QueueName queueName) (c:CommandEnvelope<'TCommand>) : TaskResult<dbo.Command, IError> =
        
            let typeName = c.Payload.GetType().AssemblyQualifiedName
            let (AggregateId aggId) = c.AggregateId
            let (CommandId cmdId) = c.CommandId
            let (CausationId causId) = c.CausationId
            let (CorrelationId corrId) = c.CorrelationId
            let payload = JsonConvert.SerializeObject(c.Payload)
            let newCommand:dbo.Command = {
                    CommandId = cmdId
                    AggregateId = aggId
                    CausationId = causId
                    CorrelationId = corrId
                    ProcessId = processIdToGuidOption c.ProcessId
                    CommandPayload = payload
                    PayloadType = typeName
                    QueueName = queueName
                    ExpectedVersion = match c.ExpectedVersion with
                                      | Expected vn -> Some vn
                                      | Irrelevant -> None
            }
            let newCommandParams = {|
                    CommandId = cmdId
                    AggregateId = aggId
                    CausationId = causId
                    CorrelationId = corrId
                    ProcessId = match c.ProcessId with
                                | Some id -> id.ToString()
                                | None -> Guid.Empty.ToString()
                    CommandPayload = payload
                    PayloadType = typeName
                    QueueName = queueName
                    ExpectedVersion = match c.ExpectedVersion with
                                      | Expected vn -> vn
                                      | Irrelevant -> -1
            |}

            try
                task {
                    let query = "INSERT INTO [dbo].[Command]
                        ([ExpectedVersion],[CommandId],[AggregateId],[CausationId],[CorrelationId],[CommandPayload],[PayloadType],[ProcessId],[QueueName])
                     VALUES (@ExpectedVersion, @CommandId, @AggregateId, @CausationId, @CorrelationId, @CommandPayload, @PayloadType, @ProcessId, @QueueName)"
                    use connection = new SqlConnection(connectionString)
                    do! connection.OpenAsync()   
                    let! _ = connection.ExecuteAsync(query, newCommandParams)   
                    return Ok newCommand
                }
            with ex -> Error ex.Message :> IError |> Result.Error |> MyTask.rtn

    let commitEvents category events =
        events
            |> List.map (eventToEntity category) 
            |> TaskResult.sequenceTaskResult
            |> TaskResult.map (List.map entityToEvent)
        

    let mapRowsToEvents (reader: IDataReader): dbo.Event list =
        let mutable events = []
        while reader.Read() do
            let event:dbo.Event =
                { 
                    Id = reader.GetInt32 0
                    EventId = reader.GetGuid 1
                    AggregateId = reader.GetGuid 2
                    CausationId = reader.GetGuid 3
                    CorrelationId =  reader.GetGuid 4 
                    ProcessId = if reader.IsDBNull(5) then None else Some (reader.GetGuid 5)
                    EventPayload = reader.GetString 6 
                    PayloadType = reader.GetString 7 
                    Category = reader.GetString 8
                }
            events <- event :: events
        reader.Close() 
        List.rev events 

        
    let loadEvents number filters (extraParams:Map<string, obj>): TaskResult<dbo.Event list, IError> =
        let query = "SELECT 
                      [Id]
                      ,[EventId]
                      ,[AggregateId]
                      ,[CausationId]
                      ,[CorrelationId]
                      ,[ProcessId]
                      ,[EventPayload]
                      ,[PayloadType]
                      ,[Category]
                  FROM [Events].[dbo].[Event]
                  WHERE Id > @Number " + filters
       
        try
            task {
                let parameters = extraParams.Add ("Number", number)
                use connection = new SqlConnection(connectionString)
                use! reader = connection.ExecuteReaderAsync(query, parameters)  
                let events = mapRowsToEvents reader
                return Ok events
            }
        with ex -> Error ex.Message :> IError |> Result.Error |> MyTask.rtn
        

    let loadTypeEvents (Category category) fromNumber =
        loadEvents fromNumber " AND Category = @Category " (Map.ofList [("Category", category)])
        
    let loadAggregateEvents (Category category) fromNumber (AggregateId aggregateId) =
        loadEvents fromNumber " AND Category = @Category AND AggregateId = @AggregateId " (Map.ofList [ ("Category", category);  ("AggregateId", aggregateId)])
        
    let loadProcessEvents fromNumber toNumber (Some (ProcessId processId)) =
        loadEvents fromNumber " AND Id <= @ToNumber AND ProcessId = @ProcessId" (Map.ofList [("ToNumber", toNumber); ("ProcessId", processId)])

    let queueCommands commands = 
        commands 
            |> List.map (fun (queueName, c) -> commandToEntity queueName c) 
            |> TaskResult.sequenceTaskResult
            |> TaskResult.map (List.map entityToCommand)


    let mapRowsToCommands (reader: IDataReader): dbo.Command list =
        let mutable commands = []
        while reader.Read() do
            let command:dbo.Command =
                {  
                    ExpectedVersion = if reader.IsDBNull(0) then None else Some (reader.GetInt32 0)
                    CommandId = reader.GetGuid 1
                    AggregateId = reader.GetGuid 2
                    CausationId = reader.GetGuid 3
                    CorrelationId =  reader.GetGuid 4 
                    CommandPayload = reader.GetString 5
                    PayloadType = reader.GetString 6 
                    ProcessId = if reader.IsDBNull(7) then None else Some (reader.GetGuid 7)
                    QueueName = reader.GetString 8
                }
            commands <- command :: commands
        List.rev commands
        

    let dequeueCommands (QueueName queueName) : TaskResult<dbo.Command list, IError> = 
        let query = "SELECT 
                       [ExpectedVersion]
                      ,[CommandId]
                      ,[AggregateId]
                      ,[CausationId]
                      ,[CorrelationId]
                      ,[CommandPayload]
                      ,[PayloadType]
                      ,[ProcessId]
                      ,[QueueName]
                  FROM [Events].[dbo].[Command]
                  WHERE QueueName = @QueueName"
        try
            task {
                use connection = new SqlConnection(connectionString)
                do! connection.OpenAsync()  

                let parameters = {|QueueName = queueName|}
                use! reader = connection.ExecuteReaderAsync(query, parameters)  
                let cmds = mapRowsToCommands reader
                do! reader.CloseAsync()  
                
                
                use deleteConnection = new Microsoft.Data.SqlClient.SqlConnection(connectionString)
                do! deleteConnection.OpenAsync()  
                let deleteQuery = "DELETE FROM [dbo].[Command] WHERE QueueName=@QueueName"
                let! _ = deleteConnection.ExecuteAsync(deleteQuery, parameters )  
                
                return Ok cmds
            }
        with ex -> Error ex.Message :> IError |> Result.Error |> MyTask.rtn
        

module Events =
    let commitEvents<'TEvent when 'TEvent :> IEvent> category (events : EventEnvelope<'TEvent> list) : TaskResult<EventEnvelope<'TEvent> list, IError> =
        try
            DataAccess.commitEvents category events
        with ex -> DataAccessError ex.Message :> IError |> Result.Error |> MyTask.rtn

    let loadAllEvents number : TaskResult<EventEnvelope<IEvent> list, IError> =
        try
            DataAccess.loadEvents number "" (Map.ofList [])
            |> TaskResult.map(List.map DataAccess.entityToEvent)
            
        with ex -> DataAccessError ex.Message :> IError |> Result.Error |> MyTask.rtn

    let loadTypeEvents category number : TaskResult<EventEnvelope<'TEvent> list, IError> =
        try
            DataAccess.loadTypeEvents category number
            |> TaskResult.map(List.map DataAccess.entityToEvent)
            
        with ex -> DataAccessError ex.Message :> IError |> Result.Error |> MyTask.rtn

    let loadAggregateEvents category number aggregateId : TaskResult<EventEnvelope<'TEvent> list, IError> =
        try
            DataAccess.loadAggregateEvents category number aggregateId
            |> TaskResult.map(List.map DataAccess.entityToEvent)
            
        with ex -> DataAccessError ex.Message :> IError |> Result.Error |> MyTask.rtn

    let loadProcessEvents fromNumber toNumber processId =
        try
            DataAccess.loadProcessEvents fromNumber toNumber processId
            |> TaskResult.map(List.map DataAccess.entityToEvent)
            
        with ex -> DataAccessError ex.Message :> IError |> Result.Error |> MyTask.rtn

module Commands =
    let queueCommands<'TCommand when 'TCommand :> ICommand> (commands: (QueueName * CommandEnvelope<'TCommand>)list): TaskResult<CommandEnvelope<'TCommand> list, IError> =
        try
           DataAccess.queueCommands commands 
        with ex -> DataAccessError ex.Message :> IError |> Result.Error |> MyTask.rtn

    let dequeueCommands<'TCommand when 'TCommand :> ICommand> queueName : TaskResult<CommandEnvelope<'TCommand> list, IError> =
        try
            DataAccess.dequeueCommands queueName
            |> TaskResult.map(List.map DataAccess.entityToCommand)
        with ex -> DataAccessError ex.Message :> IError |> Result.Error |> MyTask.rtn
