module Microsoft.eShopOnContainers.Services.Catalog.Core.CatalogBrandAggregate

open Chessie.ErrorHandling
open Microsoft.eShopOnContainers.Services.Catalog.Core.Commands
open Microsoft.eShopOnContainers.Services.Catalog.Core.Types
open System

let catelogBrandCategory = Category "CatalogBrand"
let catalogBrandQueueName = QueueName "CatalogBrand"

type CatalogBrand = {
    Id: Guid
    Brand: string
  }

 type CatalogBrandCreation = {
    Brand : string
 }

type CatalogBrandError =
    | ValidationError of string
    interface IError
    override e.ToString() = sprintf "%A" e

type Command =
    | Create of CatalogBrandCreation
    | UpdateBrand of string
    | Delete of Guid
    interface ICommand

type Event =
    | CatalogBrandCreated of CatalogBrandCreation
    | BrandUpdated of string
    | CatalogBrandDeleted of Guid
    interface IEvent

module Handlers =
    type State =
            { Created: bool 
              Item: CatalogBrand }
            static member Zero =
                { Created = false
                  Item = {
                      Id = Guid.Empty
                      Brand = ""
                  }}

    let applyEvent state event =
            match event with
            | CatalogBrandCreated item -> 
                let newBrand = { state.Item with Brand = item.Brand }
                { state with Created = true; Item = newBrand }
            | BrandUpdated newBrand -> { state with Item = { state.Item with Brand = newBrand } }
            | CatalogBrandDeleted _ -> State.Zero
  
    module private Validate =
            let validate predicate error value =
                let result = predicate value
                match result with
                | false -> ok result
                | true -> Bad [ValidationError error :> IError]


            module private Helpers =
                let notCreated  = validate (fun s -> s.Created) "Brand already created"
                let withBrand = validate (fun s -> s.Brand = "") "Brand description cannot be empty"
                let withBrandDescription = validate (fun s -> s = "") "Description cannot be empty"
                let created = validate (fun s -> not s.Created) "Brand must be created"
            let inline (<*) a b = lift2 (fun z _ -> z) a b
        
            let canCreate state item = Helpers.notCreated state  <* Helpers.withBrand item 
            let canUpdateBrand state typeF = Helpers.created state <* Helpers.withBrand typeF 
            let canUpdateBrandDescription state typeDescription = Helpers.created state <* Helpers.withBrandDescription typeDescription 
            let canDelete state = Helpers.created state
    
    let inline (<?>) a b = lift2 (fun _ z -> z) a (ok b)
    
    let executeCommand state command = 
            match command with
            | Create item -> (Validate.canCreate state item) <?> (Validate.canUpdateBrand state item) <?> [CatalogBrandCreated item]
            | UpdateBrand brandDescription -> (Validate.canUpdateBrandDescription state brandDescription) <?> [BrandUpdated brandDescription]
            | Delete id -> (Validate.canDelete state) <?> [CatalogBrandDeleted id]

let makeCatalogBrandCommandHandler =
    makeCommandHandler { Zero = Handlers.State.Zero
                         ApplyEvent = Handlers.applyEvent
                         ExecuteCommand = Handlers.executeCommand }

