namespace Microsoft.eShopOnContainers.Services.Catalog.API.ReadModel

open FSharp.Data.Sql
open System
open Chessie.ErrorHandling
open Microsoft.eShopOnContainers.Services.Catalog.API.Types
open Microsoft.eShopOnContainers.Services.Catalog.API.CatalogItemAggregate
open Microsoft.eShopOnContainers.Services.Catalog.SqlServer

type CatalogItemDTO = {
    Id : Guid
    Name : string
    Description : string
    Price: decimal
    PictureFileName : string option
    PictureUri: string option
    CatalogTypeId : Guid
    CatalogBrandId: Guid
    AvailableStock: decimal
    ReStockThreshold: decimal
    MaxStockThreshold: decimal
    OnReorder: bool
  }

module private DataAccess =
    [<Literal>]
    let connectionString = "Server=localhost;Database=Catalog;User=sa;Password=Welcome1$"

    type dbSchema = SqlDataProvider<Common.DatabaseProviderTypes.MSSQLSERVER, connectionString, UseOptionTypes = true   >
    let ctx = dbSchema.GetDataContext()

    let loadLastEvent() =
        let r =
            query {
                for ci in ctx.Dbo.CatalogItem do
                    sortByDescending ci.LastEventNumber 
                    take 1
                    select ci.LastEventNumber
            }
            |> Seq.toList

        try
            match r with
            | [] -> ok 0
            | _ -> ok (r.Head)
        with ex -> Bad [ Error ex.Message :> IError ]
    
    let insertCatalogItem (event: EventEnvelope<Event>) (catalogItem:CatalogItemCreation) =
        let (AggregateId id ) = event.AggregateId
        let newCatalogItem = ctx.Dbo.CatalogItem.Create()
        newCatalogItem.Id <- id
        newCatalogItem.Name <- catalogItem.Name
        newCatalogItem.Description <- catalogItem.Description
        newCatalogItem.Price <- 0.0m
        newCatalogItem.PictureFileName <- None
        newCatalogItem.PictureUri <- None
        newCatalogItem.CatalogTypeId <- catalogItem.CatalogTypeId
        newCatalogItem.CatalogBrandId <- catalogItem.CatalogBrandId
        newCatalogItem.AvailableStock  <- 0.0m
        newCatalogItem.RestockThreshold  <- 0.0m
        newCatalogItem.MaxStockThreshold <- 0.0m
        newCatalogItem.OnReorder <- false
        try
            ctx.SubmitUpdates()
            ok event
        with ex -> Bad [ Error ex.Message :> IError ]
    
    let doUpdate (event:EventEnvelope<Event>) update =
        let (AggregateId id) = event.AggregateId
        query {
            for ci in ctx.Dbo.CatalogItem do
                where (ci.Id = id)
        }
        |> Seq.toList
        |> List.iter update
        try
            ctx.SubmitUpdates()
            ok event
        with ex -> Bad [Error ex.Message :> IError ]

    let setPrice event price =
        let update (ci: dbSchema.dataContext.``dbo.CatalogItemEntity``) =
           ci.Price <- price
        doUpdate event update

    let setPicture event fileName uri =
        let update (ci:dbSchema.dataContext.``dbo.CatalogItemEntity``) =
            ci.PictureUri <- Some uri
            ci.PictureFileName <- Some fileName
        doUpdate event update
      
    let setStockSettings event reStock maxStock =
        let update (ci:dbSchema.dataContext.``dbo.CatalogItemEntity``) =
            ci.RestockThreshold <- reStock
            ci.MaxStockThreshold <- maxStock
        doUpdate event update

    let setStock event stock =
        let update (ci:dbSchema.dataContext.``dbo.CatalogItemEntity``) =
            ci.AvailableStock <- stock
        doUpdate event update

    let setOnReorder event onReorder =
        let update (ci:dbSchema.dataContext.``dbo.CatalogItemEntity``) =
            ci.OnReorder <- onReorder
        doUpdate event update
    
    let deleteCatalogItem (event:EventEnvelope<Event>) =
        let (AggregateId id) = event.AggregateId
        query {
            for ci in ctx.Dbo.CatalogItem do
                where (ci.Id = id)
        }
        |> Seq.toList
        |> List.iter (fun r -> r.Delete())
        try
            ctx.SubmitUpdates()
            ok event
        with ex -> Bad [Error ex.Message :> IError ]

    let loadCatalogItems () =
        query {
            for p in ctx.Dbo.CatalogItem do
                join ct in ctx.Dbo.CatalogType 
                    on (p.CatalogTypeId = ct.Id)
                join cb in ctx.Dbo.CatalogBrand
                    on (p.CatalogBrandId = cb.Id)
                select { Id = p.Id
                         Name = p.Name
                         Description = p.Description
                         Price = p.Price
                         PictureFileName = p.PictureFileName 
                         PictureUri = p.PictureUri
                         CatalogType = { Id = p.CatalogTypeId; Type = ct.Type }
                         CatalogBrand = { Id = p.CatalogBrandId; Brand = cb.Brand  }
                         AvailableStock  = p.AvailableStock
                         ReStockThreshold  = p.RestockThreshold
                         MaxStockThreshold = p.MaxStockThreshold
                         OnReorder = p.OnReorder } 
        }

module Writer = 
    module private Helpers =
        let handler (event: EventEnvelope<Event>) =
            match event.Payload with
            | CatalogItemdCreated catalogItem -> DataAccess.insertCatalogItem event catalogItem
            | PriceChanged price -> DataAccess.setPrice event price
            | PictureChanged (fileName, uri) -> DataAccess.setPicture event fileName uri
            | StockThresholdChanged (reStock, maxStock) -> DataAccess.setStockSettings event reStock maxStock
            | StockChanged stock -> DataAccess.setStock event stock
            | OnReorderSet -> DataAccess.setOnReorder event true
            | NotReorderSet -> DataAccess.setOnReorder event false
            | CatalogItemDeleted _ -> DataAccess.deleteCatalogItem event
            | _ -> Ok(event, [Error "Skipped" :> IError])

    let handleEvents() =
        let events = DataAccess.loadLastEvent() >>= Events.loadTypeEvents catelogItemCategory
        Seq.map Helpers.handler <!> events

module Reader =
    let getCatalogItems() = DataAccess.loadCatalogItems() |> Seq.toList