﻿module Microsoft.eShopOnContainers.Services.Catalog.Core.Types

    open System
    open Chessie.ErrorHandling

    type AggregateId = AggregateId of Guid
    type EventId = EventId of Guid
    type CommandId = CommandId of Guid
    type FailureId = FailureId of Guid
    type CausationId = CausationId of Guid
    type CorrelationId = CorrelationId of Guid
    type ProcessId = ProcessId of Guid
    type QueueName = QueueName of string
    type Category = Category of string
    type AggregateVersion =
        | Expected of int
        | Irrelevant
    type EventNumber = int
    // Marker interfaces: https://stackoverflow.com/questions/411709/how-do-you-declare-and-consume-marker-interface-in-f
    type IEvent = interface end
    type ICommand = interface end
    type IError = interface end

    type CatalogItemId =
        | CatalogItemId of Guid
        static member FromAggregateId (AggregateId aggrgateId) = CatalogItemId aggrgateId

    type Error =
        | Error of string
        interface IError
        override e.ToString() = sprintf "%A" e

    type EventEnvelope<'TEvent when 'TEvent :> IEvent> =
        { AggregateId : AggregateId
          Payload: 'TEvent
          EventId : EventId
          ProcessId : ProcessId option
          CausationId : CausationId
          CorrelationId : CorrelationId
          EventNumber : EventNumber }

    type CommandEnvelope<'TCommand when 'TCommand :> ICommand> =
        { AggregateId : AggregateId
          Payload: 'TCommand
          CommandId : CommandId
          ProcessId : ProcessId option
          CausationId : CausationId
          CorrelationId : CorrelationId
          ExpectedVersion : AggregateVersion }        

    type Aggregate<'TState, 'TCommand, 'TEvent> =
        { Zero : 'TState
          ApplyEvent : 'TState -> 'TEvent -> 'TState
          ExecuteCommand: 'TState -> 'TCommand -> Result<'TEvent list, IError> }

    type ProcessManager<'TState> =
        { Zero : 'TState
          ApplyEvent : 'TState -> EventEnvelope<IEvent>-> 'TState
          ProcessEvent: 'TState -> EventEnvelope<IEvent> -> Result<(QueueName * CommandEnvelope<ICommand>) list, IError> }