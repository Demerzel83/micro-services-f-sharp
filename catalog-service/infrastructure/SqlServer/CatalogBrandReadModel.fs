namespace Microsoft.eShopOnContainers.Services.Catalog.Infrastructure.SqlServer.CatalogBrandReadModel

open FSharp.Data.Sql
open System
open Chessie.ErrorHandling
open Microsoft.eShopOnContainers.Services.Catalog.Core.Types
open Microsoft.eShopOnContainers.Services.Catalog.Core.CatalogBrandAggregate
open Microsoft.eShopOnContainers.Services.Catalog.Infrastructure.SqlServer

type CatalogBrandDTO = {
    Id : Guid
    Brand : string
  }

module private DataAccess =
    [<Literal>]
    let connectionString = "Server=localhost;Database=Catalog;User=sa;Password=Welcome1$"

    type dbSchema = SqlDataProvider<Common.DatabaseProviderTypes.MSSQLSERVER, connectionString, UseOptionTypes = Common.NullableColumnType.OPTION>
    let ctx = dbSchema.GetDataContext()

    let loadLastEvent() =
        let r =
            query {
                for ci in ctx.Dbo.CatalogBrand do
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
    
    let insertCatalogBrand (event: EventEnvelope<Event>) (catalogItem:CatalogBrandCreation) =
        let (AggregateId id ) = event.AggregateId
        let newCatalogBrand = ctx.Dbo.CatalogBrand.Create()
        newCatalogBrand.Id <- id
        newCatalogBrand.Brand <- catalogItem.Brand
        try
            ctx.SubmitUpdates()
            ok event
        with ex -> Bad [ Error ex.Message :> IError ]
    
    let doUpdate (event:EventEnvelope<Event>) update =
        let (AggregateId id) = event.AggregateId
        query {
            for ci in ctx.Dbo.CatalogBrand do
                where (ci.Id = id)
        }
        |> Seq.toList
        |> List.iter update
        try
            ctx.SubmitUpdates()
            ok event
        with ex -> Bad [Error ex.Message :> IError ]

    let setBrand event brand =
        let update (ci: dbSchema.dataContext.``dbo.CatalogBrandEntity``) =
           ci.Brand <- brand
        doUpdate event update

    let deleteCatalogBrand (event:EventEnvelope<Event>) =
        let (AggregateId id) = event.AggregateId
        query {
            for ci in ctx.Dbo.CatalogBrand do
                where (ci.Id = id)
        }
        |> Seq.toList
        |> List.iter (fun r -> r.Delete())
        try
            ctx.SubmitUpdates()
            ok event
        with ex -> Bad [Error ex.Message :> IError ]

    let queryAll filter = 
        query {
            for p in ctx.Dbo.CatalogBrand do
                where  (filter p)
                select { Id = p.Id
                         Brand = p.Brand } 
        }

    let loadCatalogBrands () = queryAll (fun _ -> true)
        
    let loadCatalogBrandById id = queryAll (fun p -> p.Id = id)

    let loadCatalogBrandsByName name = queryAll (fun p -> p.Brand = name)

module Writer = 
    module private Helpers =
        let handler (event: EventEnvelope<Event>) =
            match event.Payload with
            | CatalogBrandCreated catalogBrand -> DataAccess.insertCatalogBrand event catalogBrand
            | BrandUpdated brand -> DataAccess.setBrand event brand
            | CatalogBrandDeleted _ -> DataAccess.deleteCatalogBrand event
            | _ -> Ok(event, [Error "Skipped" :> IError])

    let handleEvents() =
        let events = DataAccess.loadLastEvent() >>= Events.loadTypeEvents catelogBrandCategory
        Seq.map Helpers.handler <!> events

module Reader =
    let listResultToOption (result:Linq.IQueryable<CatalogBrandDTO>) =
        match result |> Seq.toList with
        | [] -> Option.None
        | xs -> xs |> Option.Some 

    let getCatalogbrands() = DataAccess.loadCatalogBrands() |> Seq.toList
    let getCatalogbrandById = DataAccess.loadCatalogBrandById >> listResultToOption
    let getCatalogbrandByName = DataAccess.loadCatalogBrandsByName >> listResultToOption