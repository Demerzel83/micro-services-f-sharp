namespace Microsoft.eShopOnContainers.Services.Catalog.Infrastructure.MongoDb

open Microsoft.eShopOnContainers.Services.Catalog.Core.Types
open System
open Chessie.ErrorHandling
open Newtonsoft.Json
open MongoDB.Driver
//open FSharp.Configuration
open MongoDB.Bson
open BagnoDB
open Microsoft.FSharp.Control
open System.Threading

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
        //EventNumber : int
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

    //let client = MongoClient(ConnectionString)
    //let db = client.GetDatabase(DbName)
    //let eventsCollection = db.GetCollection<EventDto>(EventsCollectionName)
    //let commandsCollection = db.GetCollection<CommandDto>(CommandsCollectionName)
    let collection = "CatalogItems"
    let database = "CatalogItems"
    
    let config = {
      host = "127.0.0.1"
      port = 27017
      user = None
      password = None
      authDb = None
    }

    
    
    let entityToEvent<'TEvent when 'TEvent :> IEvent>(e: EventDto) =
        let pid =
            match e.ProcessId with
            | Some p -> Some (ProcessId p)
            | None -> None
    
        { 
          EventNumber = e.Id
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
            //EventNumber = e.EventNumber
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
        //eventsCollection.InsertMany(entities)
        let options = InsertManyOptions()
        async {
            do! Connection.host config
                |> Connection.database database
                |> Connection.collection collection
                |> Query.insertMany CancellationToken.None options entities 
        
            return List.map entityToEvent entities
        }
        
    let connection = 
        Connection.host config
        |> Connection.database database
        |> Connection.collection collection

    let loadEvents number =
        //let filter = Builders<EventDto>.Filter.Gt((fun ci -> ci.EventNumber), number)
        //eventsCollection.Find(filter).ToEnumerable()
        let filterGt = Filter.gt (fun (o: EventDto) -> o.Id) number
        let filterOpt = FindOptions<EventDto>()
        connection
        |> Query.filter CancellationToken.None filterOpt filterGt
    
    let loadTypeEvents (Category category) (fromNumber:int) =
        //let filterGt = Builders<EventDto>.Filter.Gt((fun ci -> ci.EventNumber), fromNumber)
        //let filterEq = Builders<EventDto>.Filter.Eq((fun ci -> ci.Category), category)
        //let filter = Builders<EventDto>.Filter.And(filterGt, filterEq)
        //eventsCollection.Find(filter).ToEnumerable()
        let filterGt = Filter.gt (fun (o: EventDto) -> o.Id) fromNumber
        let filterEq = Filter.eq(fun (o:EventDto) -> o.Category) category
        let filter =  filterGt &&& filterEq
        let filterOpt = FindOptions<EventDto>()
        //async {
            //let! result =
        connection
        |> Query.filter CancellationToken.None filterOpt filter
            //return result
        //} |> Async.StartAsTask
    
    let loadAggregateEvents category fromNumber (AggregateId aggregateId) =
        //let filterGt = Builders<EventDto>.Filter.Gt((fun ci -> ci.EventNumber), fromNumber)
        //let filterEq = Builders<EventDto>.Filter.Eq((fun ci -> ci.Category), category)
        //let filterEq2 = Builders<EventDto>.Filter.Eq((fun ci -> ci.AggregateId), aggregateId)
        //let filter = Builders<EventDto>.Filter.And(filterGt, filterEq, filterEq2)
        let filterGt = Filter.gt (fun (o: EventDto) -> o.Id) fromNumber
        let filterEq = Filter.eq(fun (o:EventDto) -> o.Category) category
        let filterEq2 = Filter.eq(fun (o:EventDto) -> o.AggregateId) aggregateId
        let filter = filterGt &&& filterEq &&& filterEq2
        let filterOpt = FindOptions<EventDto>()
        connection
        |> Query.filter CancellationToken.None filterOpt filter
        //eventsCollection.Find(filter).ToEnumerable()
    
    let loadProcessEvents fromNumber toNumber (ProcessId processId) =
        //let filterGt = Builders<EventDto>.Filter.Gt((fun ci -> ci.EventNumber), fromNumber)
        //let filterEq = Builders<EventDto>.Filter.Lte((fun ci -> ci.EventNumber), fromNumber)
        //let filterEq2 = Builders<EventDto>.Filter.Eq((fun ci -> ci.ProcessId), Some processId)
        //let filter = Builders<EventDto>.Filter.And(filterGt, filterEq, filterEq2)
        //eventsCollection.Find(filter).ToEnumerable()
        let filterGt = Filter.gt (fun (o: EventDto) -> o.Id) fromNumber
        let filterEq = Filter.lte(fun (o:EventDto) -> o.Id) toNumber
        let filterEq2 = Filter.eq(fun (o:EventDto) -> o.ProcessId) (Some processId)
        let filter = filterGt &&& filterEq &&& filterEq2
        let filterOpt = FindOptions<EventDto>()
        connection
        |> Query.filter CancellationToken.None filterOpt filter
    
    let queueCommands commands = 
        let entities = List.map (fun (queueName, c) -> commandToEntity queueName c) commands
        //commandsCollection.InsertMany(entities)
        let options = InsertManyOptions()
        async {
            do! connection
                |> Query.insertMany CancellationToken.None options entities 
              
            return List.map entityToCommand entities
        }
        
    
    let dequeueCommands (QueueName queueName) = 
        //let filterEq = Builders<CommandDto>.Filter.Eq((fun ci -> ci.QueueName), queueName)
        //let filterEq = Filter.eq(fun (o:CommandDto) -> o.QueueName) queueName
        let filterEq = Filter.eq(fun (o:CommandDto) -> o.QueueName) queueName
        //let commands = commandsCollection.Find(filterEq).ToEnumerable()
        let filterOpt = FindOptions<CommandDto>()
        async {
            let! commands = 
                Connection.host config
                |> Connection.database database
                |> Connection.collection "Commands"
                |> Query.filter CancellationToken.None filterOpt filterEq
            //commandsCollection.DeleteMany(filterEq) |> ignore
            let options = DeleteOptions()
            do! Connection.host config
                |> Connection.database database
                |> Connection.collection "Commands"
                |> Query.deleteMany CancellationToken.None options filterEq
                |> Async.Ignore

            return commands
        }
        //commands
    
module Events =
    let commitEvents<'TEvent when 'TEvent :> IEvent> category (events : EventEnvelope<'TEvent> list) : Async<Result<EventEnvelope<'TEvent> list, IError>> =
        try
            async {
                let! result = DataAccess.commitEvents category events 
                return (ok result) 
            
            } 
        with ex -> async { return (Bad [DataAccessError ex.Message :> IError ]) } 
    
    let loadAllEvents number : Async<Result<EventEnvelope<IEvent> list, IError>> =
        try
            async {
                let! events = DataAccess.loadEvents number
                let result = events |> List.ofSeq |> List.map DataAccess.entityToEvent
                return (ok result)
            } 
        with ex -> async { return Bad [DataAccessError ex.Message :> IError ] } 
    
    let loadTypeEvents category number : Async<Result<EventEnvelope<'TEvent> list, IError>> =
        try
            async {
                let! typeEvents = DataAccess.loadTypeEvents category number
                let result = typeEvents |> List.ofSeq |> List.map DataAccess.entityToEvent
                return (ok result)
            } 
        with ex -> async { return (Bad [DataAccessError ex.Message :> IError ])} 
    
    let loadAggregateEvents category number aggregateId : Async<Result<EventEnvelope<'TEvent> list, IError>> =
        try
            async {
                let! events = DataAccess.loadAggregateEvents category number aggregateId
                let result = events |> List.ofSeq |> List.map DataAccess.entityToEvent
                return (ok result)
            } 
        with ex -> async { return (Bad [ DataAccessError ex.Message :> IError ] )} 
    
    let loadProcessEvents fromNumber toNumber processId =
        try
            async {
                let! events = DataAccess.loadProcessEvents fromNumber toNumber processId
                let result = events  |> List.ofSeq|> List.map DataAccess.entityToEvent
                return (ok result)
            } 
        with ex -> async { return (Bad [ DataAccessError ex.Message :> IError ]) } 
    
module Commands =
    let queueCommands<'TCommand when 'TCommand :> ICommand> (commands: (QueueName * CommandEnvelope<'TCommand>)list): Async<Result<CommandEnvelope<'TCommand> list, IError>> =
        try
            async {
                let! result = DataAccess.queueCommands commands 
                return (ok result)
            } 
        with ex -> async { return (Bad [ DataAccessError ex.Message :> IError ]) } 
    
    let dequeueCommands<'TCommand when 'TCommand :> ICommand> queueName : Async<Result<CommandEnvelope<'TCommand> list, IError>> =
        try
            async {
                let! commands = DataAccess.dequeueCommands queueName
                let result = commands |> List.ofSeq |> List.map DataAccess.entityToCommand
                return (ok result)
            } 
        with ex -> async { return (Bad [ DataAccessError ex.Message :> IError ])} 
    
