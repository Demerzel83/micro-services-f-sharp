namespace Microsoft.eShopOnContainers.Services.Catalog.Infrastructure.MongoDb.CatalogTypeReadModel


open FSharp.Data.Sql
open System
open Chessie.ErrorHandling
open Microsoft.eShopOnContainers.Services.Catalog.Core.Types
open Microsoft.eShopOnContainers.Services.Catalog.Core.CatalogTypeAggregate
open Microsoft.eShopOnContainers.Services.Catalog.Infrastructure.MongoDb

//type CatalogTypeDTO = {
//    Id : Guid
//    Type : string
//  }

module private DataAccess =
    [<Literal>]
    let connectionString = "Server=localhost;Database=Catalog;User=sa;Password=Welcome1$"

    type dbSchema = SqlDataProvider<Common.DatabaseProviderTypes.MSSQLSERVER, connectionString, UseOptionTypes = true   >
    let ctx = dbSchema.GetDataContext()

    let loadLastEvent() =
        let r =
            query {
                for ci in ctx.Dbo.CatalogType do
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
    
    let insertCatalogType (event: EventEnvelope<Event>) (catalogItem:CatalogTypeCreation) =
        let (AggregateId id ) = event.AggregateId
        let newCatalogType = ctx.Dbo.CatalogType.Create()
        newCatalogType.Id <- id
        newCatalogType.Type <- catalogItem.Type
        try
            ctx.SubmitUpdates()
            ok event
        with ex -> Bad [ Error ex.Message :> IError ]
    
    let doUpdate (event:EventEnvelope<Event>) update =
        let (AggregateId id) = event.AggregateId
        query {
            for ci in ctx.Dbo.CatalogType do
                where (ci.Id = id)
        }
        |> Seq.toList
        |> List.iter update
        try
            ctx.SubmitUpdates()
            ok event
        with ex -> Bad [Error ex.Message :> IError ]

    let setType event typeDescription =
        let update (ci: dbSchema.dataContext.``dbo.CatalogTypeEntity``) =
           ci.Type <- typeDescription
        doUpdate event update

    let deleteCatalogType (event:EventEnvelope<Event>) =
        let (AggregateId id) = event.AggregateId
        query {
            for ci in ctx.Dbo.CatalogType do
                where (ci.Id = id)
        }
        |> Seq.toList
        |> List.iter (fun r -> r.Delete())
        try
            ctx.SubmitUpdates()
            ok event
        with ex -> Bad [Error ex.Message :> IError ]

    let loadCatalogTypes () =
        query {
            for p in ctx.Dbo.CatalogType do
                select { Id = p.Id
                         Type = p.Type } 
        }

module Writer = 
    module private Helpers =
        let handler (event: EventEnvelope<Event>) =
            match event.Payload with
            | CatalogTypeCreated catalogType -> DataAccess.insertCatalogType event catalogType
            | TypeUpdated typeDescription -> DataAccess.setType event typeDescription
            | CatalogTypeDeleted _ -> DataAccess.deleteCatalogType event
            | _ -> Ok(event, [Error "Skipped" :> IError])

    let handleEvents() =
        let events = DataAccess.loadLastEvent() >>= Events.loadTypeEvents catelogTypeCategory
        Seq.map Helpers.handler <!> events

module Reader =
    let getCatalogTypes() = DataAccess.loadCatalogTypes() |> Seq.toList