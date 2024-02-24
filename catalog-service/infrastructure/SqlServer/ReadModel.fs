namespace Microsoft.eShopOnContainers.Services.Catalog.Infrastructure.SqlServer.ReadModel

open FSharp.Data.Sql
open System
open Chessie.ErrorHandling
open Microsoft.eShopOnContainers.Services.Catalog.Core.Types
//open Microsoft.eShopOnContainers.Services.Catalog.Core.CatalogAggregate
open Microsoft.eShopOnContainers.Services.Catalog.Core.CatalogItemAggregate
open Microsoft.eShopOnContainers.Services.Catalog.Infrastructure.SqlServer

type CatalogDTO = {
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
    let connectionString = "Server=localhost;Database=Catalog;User=sa;Password=Str0ngSPssw0rd02"

    type dbSchema = SqlDataProvider<Common.DatabaseProviderTypes.MSSQLSERVER, connectionString, UseOptionTypes = Common.NullableColumnType.OPTION>
    let ctx = dbSchema.GetDataContext()

    let loadLastEvent() =
        let r =
            query {
                for ci in ctx.Dbo.Catalog do
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
    
    let insertCatalog (event: EventEnvelope<Event>) (catalog:CatalogItemCreation) =
        let (AggregateId id ) = event.AggregateId
        let newCatalog = ctx.Dbo.Catalog.Create()
        newCatalog.Id <- id
        newCatalog.Name <- catalog.Name
        newCatalog.Description <- catalog.Description
        newCatalog.Price <- 0.0m
        newCatalog.PictureFileName <- None
        newCatalog.PictureUri <- None
        newCatalog.CatalogTypeId <- catalog.CatalogTypeId
        newCatalog.CatalogBrandId <- catalog.CatalogBrandId
        newCatalog.AvailableStock  <- 0.0m
        newCatalog.RestockThreshold  <- 0.0m
        newCatalog.MaxStockThreshold <- 0.0m
        newCatalog.OnReorder <- false
        try
            ctx.SubmitUpdates()
            ok event
        with ex -> Bad [ Error ex.Message :> IError ]
    
    let doUpdate (event:EventEnvelope<Event>) update =
        let (AggregateId id) = event.AggregateId
        query {
            for ci in ctx.Dbo.Catalog do
                where (ci.Id = id)
        }
        |> Seq.toList
        |> List.iter update
        try
            ctx.SubmitUpdates()
            ok event
        with ex -> Bad [Error ex.Message :> IError ]

    let setPrice event price =
        let update (ci: dbSchema.dataContext.``dbo.CatalogEntity``) =
           ci.Price <- price
        doUpdate event update

    let setPicture event fileName uri =
        let update (ci:dbSchema.dataContext.``dbo.CatalogEntity``) =
            ci.PictureUri <- Some uri
            ci.PictureFileName <- Some fileName
        doUpdate event update
      
    let setStockSettings event reStock maxStock =
        let update (ci:dbSchema.dataContext.``dbo.CatalogEntity``) =
            ci.RestockThreshold <- reStock
            ci.MaxStockThreshold <- maxStock
        doUpdate event update

    let setStock event stock =
        let update (ci:dbSchema.dataContext.``dbo.CatalogEntity``) =
            ci.AvailableStock <- stock
        doUpdate event update

    let setOnReorder event onReorder =
        let update (ci:dbSchema.dataContext.``dbo.CatalogEntity``) =
            ci.OnReorder <- onReorder
        doUpdate event update
    
    let deleteCatalog (event:EventEnvelope<Event>) =
        let (AggregateId id) = event.AggregateId
        query {
            for ci in ctx.Dbo.Catalog do
                where (ci.Id = id)
        }
        |> Seq.toList
        |> List.iter (fun r -> r.Delete())
        try
            ctx.SubmitUpdates()
            ok event
        with ex -> Bad [Error ex.Message :> IError ]

    let loadCatalogs () = 
        query {
            for p in ctx.Dbo.Catalog do
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

    let loadCatalogById id = 
        query {
            for p in ctx.Dbo.Catalog do
                where (p.Id = id)
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

    let loadCatalogsByDescription description = 
        query {
            for p in ctx.Dbo.Catalog do
                where (p.Description = description)
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

    let loadCatalogsByTypeAndBrand typeId brandId = 
        query {
            for p in ctx.Dbo.Catalog do
                where (p.CatalogBrandId = brandId && p.CatalogTypeId = typeId)
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

    let loadCatalogsByType typeId = 
        query {
            for p in ctx.Dbo.Catalog do
                where (p.CatalogTypeId = typeId)
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

    let loadCatalogsByBrand brandId = 
        query {
            for p in ctx.Dbo.Catalog do
                where (p.CatalogBrandId = brandId)
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
            | CatalogItemCreated catalog -> DataAccess.insertCatalog event catalog
            | PriceChanged price -> DataAccess.setPrice event price
            | PictureChanged (fileName, uri) -> DataAccess.setPicture event fileName uri
            | StockThresholdChanged (reStock, maxStock) -> DataAccess.setStockSettings event reStock maxStock
            | StockChanged stock -> DataAccess.setStock event stock
            | OnReorderSet -> DataAccess.setOnReorder event true
            | NotReorderSet -> DataAccess.setOnReorder event false
            | CatalogItemDeleted _ -> DataAccess.deleteCatalog event
            | _ -> Ok(event, [Error "Skipped" :> IError])

    let handleEvents() =
        let events = DataAccess.loadLastEvent() >>= Events.loadTypeEvents catelogItemCategory
        Seq.map Helpers.handler <!> events

module Reader =
    let getCatalogs() = DataAccess.loadCatalogs() |> Seq.toList

    let listResultToOption (result:Linq.IQueryable<CatalogItem>) =
        match result |> Seq.toList with
        | [] -> Option.None
        | xs -> xs |> Option.Some 

    let getCatalogById id = 
        let result = DataAccess.loadCatalogById id |> Seq.toList 
        match result with
        | [] -> Option.None
        | xs -> 
            xs
            |> Seq.head
            |> Option.Some 

    let getCatalogsByDescription = 
        DataAccess.loadCatalogsByDescription >> listResultToOption

    let getCatalogsByTypeAndBrand typeId brandId =
        DataAccess.loadCatalogsByTypeAndBrand typeId brandId 
        |> listResultToOption
        
    let getCatalogsByType =
        DataAccess.loadCatalogsByType >> listResultToOption
        
    let getCatalogsByBrand =
        DataAccess.loadCatalogsByBrand >> listResultToOption
        