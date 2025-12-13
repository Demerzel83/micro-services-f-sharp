module Microsoft.eShopOnContainers.Services.Catalog.Core.Commands

open System
open Microsoft.eShopOnContainers.Services.Catalog.Core.Types
open Microsoft.eShopOnContainers.Services.Catalog.Core.Events
open TaskResult
open MyResult   

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
    (load: AggregateId -> TaskResult<EventEnvelope<'TEvent> list, IError>)
    (commit: EventEnvelope<'TEvent> list -> TaskResult<EventEnvelope<'TError> list, IError>) =
    let handleCommand command : TaskResult<EventEnvelope<'TEvent> list, IError> = taskResult {
            let processEvents events : TaskResult<EventEnvelope<'TEvent> list, IError> =
                let lastEventNumber = List.fold(fun acc e' -> e'.EventNumber) 0 events
                   
                match lastEventNumber,command.ExpectedVersion with
                | (_, Irrelevant) ->  Error "Irrelevant Version" :> IError |> Result.Error   |> MyTask.rtn
                | (lastEventNumber, Expected y) when lastEventNumber > y -> Error "Version mistmatch" :> IError |> Result.Error  |> MyTask.rtn
                | _ -> taskResult {
                        let eventPayloads = List.map (fun (e: EventEnvelope<'TEvent>) -> e.Payload) events
                        let state = List.fold aggregate.ApplyEvent aggregate.Zero eventPayloads
                        let! events  = aggregate.ExecuteCommand state command.Payload |> MyTask.rtn :>  TaskResult<IEvent list, IError>
                        let result2 = events |> List.map (fun e -> createEventMetadata e command) 
                        let! result3 =  commit result2
                        return result3
                    }
                   
                    
        
            let id = command.AggregateId
            let loadEvents = load id
            let! r = TaskResult.bind processEvents loadEvents 
            return r
        }
    handleCommand
