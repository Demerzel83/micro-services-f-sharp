module Microsoft.eShopOnContainers.Services.Catalog.Core.CatalogTypeAggregate

open Chessie.ErrorHandling
open Microsoft.eShopOnContainers.Services.Catalog.Core.Commands
open Microsoft.eShopOnContainers.Services.Catalog.Core.Types
open System

let catelogTypeCategory = Category "CatalogType"
let catalogTypeQueueName = QueueName "CatalogType"

type CatalogType = {
    Id: Guid
    Type: string
  }

type CatalogTypeCreation = {
    Type: string
}

type CatalogItemError =
    | ValidationError of string
    interface IError
    override e.ToString() = sprintf "%A" e

type Command =
    | Create of CatalogTypeCreation
    | UpdateType of string
    | Delete of Guid
    interface ICommand

type Event =
    | CatalogTypeCreated of CatalogTypeCreation
    | TypeUpdated of string
    | CatalogTypeDeleted of Guid
    interface IEvent

module Handlers =
    type State =
            { Created: bool 
              Item: CatalogType }
            static member Zero =
                { Created = false
                  Item = {
                      Id = Guid.Empty
                      Type = ""
                  }}

    let applyEvent state event =
            match event with
            | CatalogTypeCreated item -> 
                let newItem = { state.Item with Type = item.Type }
                { state with Created = true; Item = newItem}
            | TypeUpdated newType -> { state with Item = { state.Item with Type = newType } }
            | CatalogTypeDeleted _ -> State.Zero
  
    module private Validate =
            let validate predicate error value =
                let result = predicate value
                match result with
                | false -> ok result
                | true -> Bad [ValidationError error :> IError]


            module private Helpers =
                let notCreated  = validate (fun s -> s.Created) "Type already created"
                let withType = validate (fun s -> s.Type = "") "Type description cannot be empty"
                let withTypeDescription = validate (fun s -> s = "") "Description cannot be empty"
                let created = validate (fun s -> not s.Created) "Type must be created"
            let inline (<*) a b = lift2 (fun z _ -> z) a b
        
            let canCreate state item = Helpers.notCreated state  <* Helpers.withType item 
            let canUpdateType state typeF = Helpers.created state <* Helpers.withType typeF 
            let canUpdateTypeDescription state typeDescription = Helpers.created state <* Helpers.withTypeDescription typeDescription 
            let canDelete state = Helpers.created state
    
    let inline (<?>) a b = lift2 (fun _ z -> z) a (ok b)
    
    let executeCommand state command = 
            match command with
            | Create item -> (Validate.canCreate state item) <?> (Validate.canUpdateType state item) <?> [CatalogTypeCreated item]
            | UpdateType typeDescription -> (Validate.canUpdateTypeDescription state typeDescription) <?> [TypeUpdated typeDescription]
            | Delete id -> (Validate.canDelete state) <?> [CatalogTypeDeleted id]

let makeCatalogTypeCommandHandler =
    makeCommandHandler { Zero = Handlers.State.Zero
                         ApplyEvent = Handlers.applyEvent
                         ExecuteCommand = Handlers.executeCommand }