module Microsoft.eShopOnContainers.Services.Catalog.Core.Events

open System
open Microsoft.eShopOnContainers.Services.Catalog.Core.Types
open MyResult
open TaskResult

let createEvent aggregateId (causationId, processId, correlationId) payload =
    { AggregateId = aggregateId
      Payload = payload
      EventId = Guid.NewGuid() |> EventId 
      ProcessId = processId
      CausationId = causationId
      CorrelationId = correlationId
      EventNumber = 0 }

let createEventMetadata payload command =
    let (CommandId cmdGuid) = command.CommandId
    { AggregateId = command.AggregateId
      Payload = payload
      EventId = Guid.NewGuid() |> EventId
      ProcessId = command.ProcessId
      CausationId = CausationId cmdGuid
      CorrelationId = command.CorrelationId
      EventNumber = 0 }

let makeEventProcessor (processManager : ProcessManager<'TState>)
    (load : ProcessId -> Result<EventEnvelope<IEvent> list, IError>)
    (enqueue : (QueueName * CommandEnvelope<ICommand>) list -> Result<CommandEnvelope<ICommand> list, IError>) =
    let handleEvent (event : EventEnvelope<IEvent>) : TaskResult<CommandEnvelope<ICommand> list, IError> =
        let processEvents events = taskResult {
            let state = List.fold processManager.ApplyEvent processManager.Zero events
            let! result = processManager.ProcessEvent state event
            let! result2 = enqueue result |> MyTask.rtn
            return result2
        }

        match event.ProcessId with
        | Some pid -> taskResult {
                let! loadedEvents = load pid |> MyTask.rtn
                let! result = processEvents loadedEvents
                return result
            }
        | _ -> TaskResult.rtn []
    handleEvent
