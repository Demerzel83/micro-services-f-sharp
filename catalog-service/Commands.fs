module Microsoft.eShopOnContainers.Services.Catalog.API.Commands

open System
open Chessie.ErrorHandling
open Microsoft.eShopOnContainers.Services.Catalog.API.Types
open Microsoft.eShopOnContainers.Services.Catalog.API.Events

let createCommand aggregateId (version, causationId, correlationId, processId) payload =
    let commandId = Guid.NewGuid()

    let causationId' =
        match causationId with
        | Some c -> c
        | _ -> CausationId commandId

    let correlactionId' =
        match correlationId with
        | Some c -> c
        | _ -> CorrelationId commandId

    { AggregateId = aggregateId
      Payload = payload
      CommandId = CommandId commandId
      ProcessId = processId
      CausationId = causationId'
      CorrelationId = correlactionId'
      ExpectedVersion = version }

let makeCommandHandler (aggregate: Aggregate<'TState, 'TCommand, 'TEvent>)
    (load: AggregateId -> Result<EventEnvelope<'TEvent> list, IError>)
    (commit: EventEnvelope<'TEvent> list -> Result<EventEnvelope<'TError> list, IError>) =
    let handleCommand command : Result<EventEnvelope<'TEvent> list, IError> =
        let processEvents events =
            let lastEventNumber = List.fold(fun acc e' -> e'.EventNumber) 0 events
            let e = lastEventNumber
            let v =
                match command.ExpectedVersion with
                | Expected v' -> Some(v')
                | Irrelevant -> None
                    
            match e,v with
            | (x, Some(y)) when x > y -> Bad [Error "Version mistmatch" :> IError]
            | _ ->
                let eventPayloads = List.map (fun (e: EventEnvelope<'TEvent>) -> e.Payload) events
                let state = List.fold aggregate.ApplyEvent aggregate.Zero eventPayloads
                let result = aggregate.ExecuteCommand state command.Payload
                List.map (fun e -> createEventMetadata e command) <!> result >>= commit
        
        let id = command.AggregateId
        let loadEvents = load id
        processEvents <!> loadEvents |> flatten
    handleCommand
