module Microsoft.eShopOnContainers.Services.Catalog.API.CatalogItemAggregate

open Chessie.ErrorHandling
open Microsoft.eShopOnContainers.Services.Catalog.API.Commands
open Microsoft.eShopOnContainers.Services.Catalog.API.Types
open System

let catelogItemCategory = Category "CatalogItem"
let catalogItemQueueName = QueueName "CatalogItem"

type CatalogType = {
    Id: Guid
    Type: string
  }

type CatalogBrand = {
    Id : Guid
    Brand : string
  }

type CatalogItem = {
    Id : Guid
    Name : string
    Description : string
    Price: decimal
    PictureFileName : string option
    PictureUri: string option
    CatalogType : CatalogType
    CatalogBrand: CatalogBrand
    AvailableStock: decimal
    ReStockThreshold: decimal
    MaxStockThreshold: decimal
    OnReorder: bool
  }

type CatalogItemCreation = {
    Name : string
    Description : string
    CatalogTypeId : Guid
    CatalogBrandId : Guid
  }

type CatalogItemError =
    | ValidationError of string
    interface IError
    override e.ToString() = sprintf "%A" e

type Command =
    | Create of CatalogItemCreation
    | SetPrice of decimal
    | SetPicture of FileName:string * Uri:string
    | UpdateStockThreshold of ReStock:decimal * MaxStock:decimal
    | SetStock of decimal
    | OnReorder
    | NotOnReorder
    | Delete of Guid
    interface ICommand

type Event =
    | CatalogItemdCreated of CatalogItemCreation
    | PriceChanged of decimal
    | PictureChanged of FileName:string * Uri:string
    | StockThresholdChanged of ReStock:decimal * MaxStock:decimal
    | StockChanged of decimal
    | OnReorderSet
    | NotReorderSet
    | CatalogItemDeleted of Guid
    interface IEvent

module Handlers =
    type State =
            { Created: bool 
              Item: CatalogItem }
            static member Zero =
                { Created = false
                  Item = {
                      Id = Guid.Empty
                      Name = ""
                      Description = ""
                      Price = 0.0m
                      PictureFileName = None
                      PictureUri = None
                      CatalogType = { Id = Guid.Empty; Type = ""}
                      CatalogBrand = { Id = Guid.Empty; Brand = "" }
                      AvailableStock = 0.0m
                      ReStockThreshold = 0.0m
                      MaxStockThreshold = 0.0m
                      OnReorder = false
                  }}

    let applyEvent state event =
            match event with
            | CatalogItemdCreated item -> 
                let newItem = { state.Item with Name = item.Name; Description = item.Description; CatalogBrand = { state.Item.CatalogBrand with Id = item.CatalogBrandId }; CatalogType = { state.Item.CatalogType with Id = item.CatalogTypeId } }
                { state with Created = true; Item = newItem}
            | PriceChanged newPrice -> { state with Item = { state.Item with Price = newPrice } }
            | PictureChanged (fileName, uri) -> { state with Item = { state.Item with PictureFileName = Some fileName; PictureUri = Some uri } }
            | StockThresholdChanged (reStock, maxStock) -> { state with Item = { state.Item with  ReStockThreshold = reStock; MaxStockThreshold = maxStock } }
            | StockChanged value -> { state with Item = { state.Item with AvailableStock = value } }
            | OnReorderSet -> { state with Item = { state.Item with OnReorder = true } }
            | NotReorderSet -> { state with Item = { state.Item with OnReorder = false } }
            | CatalogItemDeleted _ -> State.Zero
  
    module private Validate =
            let validate predicate error value =
                let result = predicate value
                match result with
                | false -> ok result
                | true -> Bad [ValidationError error :> IError]


            module private Helpers =
                let notCreated  = validate (fun s -> s.Created) "Product already created"
                let positivePrice = validate (fun price -> price < 0m) "Price must be a positive number"
                let withDescription = validate (fun s -> s.Description = "") "Product description cannot be empty"
                let withName = validate (fun s -> s.Name = "") "Product name cannot by empty"
                let withBrand = validate (fun s -> s.CatalogBrandId = Guid.Empty) "Product Brand cannot by empty"
                let withType = validate (fun s -> s.CatalogTypeId = Guid.Empty) "Product Type cannot by empty"
                let created = validate (fun s -> not s.Created) "Product must be created"
                let withFileNameAndUri = validate (fun (fileName, uri) -> fileName = "" || uri = "") "A file name and a URI are required"
                let withPositiveStockValues = validate (fun (reStock, maxStock) -> reStock < 0.0m ||  maxStock < 0.0m ) "ReStock and MaxStock values must be positive numbers"
                let positiveStock = validate(fun stock -> stock < 0.0m) "Stock must by positive"
            let inline (<*) a b = lift2 (fun z _ -> z) a b
        
            let canCreate state item = Helpers.notCreated state  <* Helpers.withBrand item <* Helpers.withType item <* Helpers.withDescription item <* Helpers.withName item
            let canSetPrice state price = Helpers.created state <* Helpers.positivePrice price 
            let canSetPicture state fileDetails = Helpers.created state <* Helpers.withFileNameAndUri fileDetails
            let canSetStockThreshold state stockdetails = Helpers.created state <* Helpers.withPositiveStockValues stockdetails
            let canSetStock state stock = Helpers.created state <* Helpers.positiveStock stock
            let canOnReorder state = Helpers.created state
            let canNotOnReorder state = Helpers.created state
            let canDelete state = Helpers.created state
    
    let inline (<?>) a b = lift2 (fun _ z -> z) a (ok b)
    
    let executeCommand state command = 
            match command with
            | Create item -> (Validate.canCreate state item) <?> [CatalogItemdCreated item]
            | SetPrice price -> (Validate.canSetPrice state price) <?> [PriceChanged price]
            | SetPicture (fileName, uri) -> (Validate.canSetPicture state (fileName, uri)) <?> [PictureChanged (fileName, uri)]
            | UpdateStockThreshold (reStock, maxStock) -> (Validate.canSetStockThreshold state (reStock, maxStock)) <?> [StockThresholdChanged (reStock, maxStock)]
            | SetStock value -> (Validate.canSetStock state value) <?> [StockChanged value]
            | OnReorder -> (Validate.canOnReorder state) <?> [OnReorderSet]
            | NotOnReorder -> (Validate.canNotOnReorder state) <?> [NotReorderSet]
            | Delete id -> (Validate.canDelete state) <?> [CatalogItemDeleted id]

let makeCatalogItemCommandHandler =
    makeCommandHandler { Zero = Handlers.State.Zero
                         ApplyEvent = Handlers.applyEvent
                         ExecuteCommand = Handlers.executeCommand }