namespace Microsoft.eShopOnContainers.Services.Catalog.Infrastructure.MongoDb

open Microsoft.eShopOnContainers.Services.Catalog.Core.Types
open System
open Chessie.ErrorHandling
open Newtonsoft.Json
open MongoDB.Driver
open FSharp.Configuration
open MongoDB.Bson


type DataAccessError =
    | DataAccessError of string
    interface IError
    
module private DataAccess =
    [<Literal>]
    let ConnectionString = "mongodb://localhost:27017/?readPreference=primary&appname=MongoDB%20Compass%20Community&ssl=false"
    //[<Literal>] 
    //let connectionString = AppSettings<"app.config">.ConnectionStrings.MongoDbConnectionString

    [<Literal>]
    let DbName = "CatalogItems"

    [<Literal>]
    let EventsCollectionName = "Events"

    [<Literal>]
    let CommandsCollectionName = "Commands"
    
    [<CLIMutable>]
    type EventDto = {  
        _id: ObjectId    
        AggregateId : Guid
        Category : string
        CausationId : Guid
        CorrelationId : Guid
        EventId : Guid
        EventPayload : string
        Id: int
        PayloadType : string
        ProcessId : Guid option
        EventNumber : int
    }

    type CommandDto = {
        AggregateId : Guid
        CausationId : Guid
        CommandId : Guid
        CommandPayload : string
        CorrelationId : Guid
        ExpectedVersion : int option
        Id: int
        PayloadType : string
        ProcessId : Guid option
        QueueName: string
    }

    let client = MongoClient(ConnectionString)
    let db = client.GetDatabase(DbName)
    let eventsCollection = db.GetCollection<EventDto>(EventsCollectionName)
    let commandsCollection = db.GetCollection<CommandDto>(CommandsCollectionName)

    
    let entityToEvent<'TEvent when 'TEvent :> IEvent>(e: EventDto) =
        let pid =
            match e.ProcessId with
            | Some p -> Some (ProcessId p)
            | None -> None
    
        { EventNumber = e.EventNumber
          EventId = EventId e.EventId
          AggregateId = AggregateId e.AggregateId
          CausationId = CausationId e.CausationId
          CorrelationId = CorrelationId e.CorrelationId
          ProcessId = pid
          Payload = JsonConvert.DeserializeObject(e.EventPayload, Type.GetType(e.PayloadType)) :?> 'TEvent}
    
    let entityToCommand<'TCommand when 'TCommand :> ICommand>(c:CommandDto) =
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
    
    let processIdToGuid pid =
        match pid with
        | Some pid ->
            let (ProcessId id) = pid
            Some id
        | _ -> None
    
    let eventToEntity<'TEvent when 'TEvent :> IEvent> (Category category) (e:EventEnvelope<'TEvent>) =
        let typeName = e.Payload.GetType().AssemblyQualifiedName
        let (AggregateId aggId) = e.AggregateId
        let (EventId evId) = e.EventId
        let (CausationId causId) = e.CausationId
        let (CorrelationId corrId) = e.CorrelationId
        let payload = JsonConvert.SerializeObject(e.Payload)
        let event = {
            _id = ObjectId.Empty
            AggregateId = aggId
            Category = category
            CausationId = causId
            CorrelationId =  corrId
            EventId = evId
            EventPayload  = payload
            Id = 666 // todo
            PayloadType = typeName
            ProcessId = processIdToGuid e.ProcessId
            EventNumber = e.EventNumber
        }
        event
    
    let commandToEntity<'TCommand when 'TCommand:> ICommand> (QueueName queueName) (c:CommandEnvelope<'TCommand>) =
        let typeName = c.Payload.GetType().AssemblyQualifiedName
        let (AggregateId aggId) = c.AggregateId
        let (CommandId cmdId) = c.CommandId
        let (CausationId causId) = c.CausationId
        let (CorrelationId corrId) = c.CorrelationId
        let payload = JsonConvert.SerializeObject(c.Payload)
        let command = {
            QueueName = queueName
            AggregateId = aggId
            CausationId = causId
            CommandId = cmdId
            CommandPayload = payload
            CorrelationId = corrId
            ExpectedVersion = 
                match c.ExpectedVersion with
                | Expected vn -> Some vn
                | Irrelevant -> None
            Id = 666 // todo
            PayloadType = typeName
            ProcessId = processIdToGuid c.ProcessId
        }
        command
    
    let commitEvents category events =
        let entities = List.map (eventToEntity category) events
        eventsCollection.InsertMany(entities)
        List.map entityToEvent entities
    
    let loadEvents number =
        let filter = Builders<EventDto>.Filter.Gt((fun ci -> ci.EventNumber), number)
        eventsCollection.Find(filter).ToEnumerable()
    
    let loadTypeEvents (Category category) fromNumber =
        let filterGt = Builders<EventDto>.Filter.Gt((fun ci -> ci.EventNumber), fromNumber)
        let filterEq = Builders<EventDto>.Filter.Eq((fun ci -> ci.Category), category)
        let filter = Builders<EventDto>.Filter.And(filterGt, filterEq)
        eventsCollection.Find(filter).ToEnumerable()
    
    let loadAggregateEvents category fromNumber (AggregateId aggregateId) =
        let filterGt = Builders<EventDto>.Filter.Gt((fun ci -> ci.EventNumber), fromNumber)
        let filterEq = Builders<EventDto>.Filter.Eq((fun ci -> ci.Category), category)
        let filterEq2 = Builders<EventDto>.Filter.Eq((fun ci -> ci.AggregateId), aggregateId)
        let filter = Builders<EventDto>.Filter.And(filterGt, filterEq, filterEq2)
        eventsCollection.Find(filter).ToEnumerable()
    
    let loadProcessEvents fromNumber toNumber (ProcessId processId) =
        let filterGt = Builders<EventDto>.Filter.Gt((fun ci -> ci.EventNumber), fromNumber)
        let filterEq = Builders<EventDto>.Filter.Lte((fun ci -> ci.EventNumber), fromNumber)
        let filterEq2 = Builders<EventDto>.Filter.Eq((fun ci -> ci.ProcessId), Some processId)
        let filter = Builders<EventDto>.Filter.And(filterGt, filterEq, filterEq2)
        eventsCollection.Find(filter).ToEnumerable()
    
    let queueCommands commands = 
        let entities = List.map (fun (queueName, c) -> commandToEntity queueName c) commands
        commandsCollection.InsertMany(entities)
        List.map entityToCommand entities
    
    let dequeueCommands (QueueName queueName) = 
        let filterEq = Builders<CommandDto>.Filter.Eq((fun ci -> ci.QueueName), queueName)
        let commands = commandsCollection.Find(filterEq).ToEnumerable()
        commandsCollection.DeleteMany(filterEq) |> ignore
        commands
    
module Events =
    let commitEvents<'TEvent when 'TEvent :> IEvent> category (events : EventEnvelope<'TEvent> list) : Result<EventEnvelope<'TEvent> list, IError> =
        try
            DataAccess.commitEvents category events |> ok
        with ex -> Bad [DataAccessError ex.Message :> IError ]
    
    let loadAllEvents number : Result<EventEnvelope<IEvent> list, IError> =
        try
            DataAccess.loadEvents number
            |> Seq.toList
            |> List.map DataAccess.entityToEvent
            |> ok
        with ex -> Bad [DataAccessError ex.Message :> IError ]
    
    let loadTypeEvents category number : Result<EventEnvelope<'TEvent> list, IError> =
        try
            DataAccess.loadTypeEvents category number
            |> Seq.toList
            |> List.map DataAccess.entityToEvent
            |> ok
        with ex -> Bad [DataAccessError ex.Message :> IError ]
    
    let loadAggregateEvents category number aggregateId : Result<EventEnvelope<'TEvent> list, IError> =
        try
            DataAccess.loadAggregateEvents category number aggregateId
            |> Seq.toList
            |> List.map DataAccess.entityToEvent
            |> ok
        with ex -> Bad [ DataAccessError ex.Message :> IError ]
    
    let loadProcessEvents fromNumber toNumber processId =
        try
            DataAccess.loadProcessEvents fromNumber toNumber processId
            |> Seq.toList
            |> List.map DataAccess.entityToEvent
            |> ok
        with ex -> Bad [ DataAccessError ex.Message :> IError ]
    
module Commands =
    let queueCommands<'TCommand when 'TCommand :> ICommand> (commands: (QueueName * CommandEnvelope<'TCommand>)list): Result<CommandEnvelope<'TCommand> list, IError> =
        try
            DataAccess.queueCommands commands 
            |> ok
        with ex -> Bad [ DataAccessError ex.Message :> IError ]
    
    let dequeueCommands<'TCommand when 'TCommand :> ICommand> queueName : Result<CommandEnvelope<'TCommand> list, IError> =
        try
            DataAccess.dequeueCommands queueName
            |> Seq.toList
            |> List.map DataAccess.entityToCommand
            |> ok
        with ex -> Bad [ DataAccessError ex.Message :> IError ]
    
