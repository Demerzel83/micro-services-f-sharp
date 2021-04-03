namespace Microsoft.eShopOnContainers.Services.Catalog.Infrastructure.MongoDb.ReadModel

open FSharp.Data.Sql
open System
open Chessie.ErrorHandling
open MongoDB.Driver
open MongoDB.Bson
open MongoDB.FSharp

open Microsoft.eShopOnContainers.Services.Catalog.Core.Types
open Microsoft.eShopOnContainers.Services.Catalog.Core.CatalogItemAggregate
open Microsoft.eShopOnContainers.Services.Catalog.Infrastructure.MongoDb
open System.Linq

type CatalogTypeDTO = {
    Id : Guid
    Type : string
  }

type CatalogBrandDTO = {
      Id : Guid
      Brand : string
    }


type CatalogItemDTO = {
    Id : Guid
    Name : string
    Description : string
    Price: decimal
    PictureFileName : string option
    PictureUri: string option
    CatalogType : CatalogTypeDTO
    CatalogBrand: CatalogBrandDTO
    AvailableStock: decimal
    ReStockThreshold: decimal
    MaxStockThreshold: decimal
    OnReorder: bool
    LastEventNumber : int
  }

  
module private DataAccess =
    [<Literal>]
    let ConnectionString = "mongodb://localhost:27017/?readPreference=primary&appname=MongoDB%20Compass%20Community&ssl=false"

    [<Literal>]
    let DbName = "CatalogItems"

    [<Literal>]
    let EventsCollectionName = "CatalogItems"
    
    let client = MongoClient(ConnectionString)
    let db = client.GetDatabase(DbName)
    let catalogItemsCollection = db.GetCollection<CatalogItemDTO>(EventsCollectionName)
    
    let loadLastEvent () = 
        try
            let result = catalogItemsCollection.Find(fun x -> x.LastEventNumber > 0).ToEnumerable().ToList()
            if result.Count = 0 
            then ok 0
            else ok (result.First().LastEventNumber)
        with ex -> Bad [ Error ex.Message :> IError ]
    
    let insertCatalogItem (event: EventEnvelope<Event>) (catalogItem:CatalogItemCreation) =
        let (AggregateId id ) = event.AggregateId
        let newCatalogItem:CatalogItemDTO = {
            Id = id
            Name = catalogItem.Name
            Description = catalogItem.Description
            Price = 0.0m
            PictureFileName = None
            PictureUri = None
            CatalogType = {
                Id = catalogItem.CatalogTypeId
                Type = "TODO"
            }
            CatalogBrand = {
                Id = catalogItem.CatalogBrandId
                Brand = "TODO"
            }
            AvailableStock  = 0.0m
            ReStockThreshold  = 0.0m
            MaxStockThreshold = 0.0m
            OnReorder = false
            LastEventNumber = 666 //?
        }
        try
            catalogItemsCollection.InsertOne(newCatalogItem)
            ok event
        with ex -> Bad [ Error ex.Message :> IError ]
    
    let doUpdate (event:EventEnvelope<Event>) update =
        let (AggregateId id) = event.AggregateId
        try
            let filter = Builders<CatalogItemDTO>.Filter.Eq((fun ci -> ci.Id), id)
            catalogItemsCollection.UpdateOne(filter, update) |> ignore
            ok event
        with ex -> 
            Bad [Error ex.Message :> IError ]

    let setPrice event price =
        let update = Builders<CatalogItemDTO>.Update.Set((fun x -> x.Price), price)
        doUpdate event update

    let setPicture event fileName uri =
        let update = Builders<CatalogItemDTO>.Update.Set((fun x -> x.PictureUri), Some uri).Set((fun x -> x.PictureFileName), Some fileName)
        doUpdate event update
      
    let setStockSettings event reStock maxStock =
        let update = Builders<CatalogItemDTO>.Update.Set((fun x -> x.ReStockThreshold), reStock).Set((fun x -> x.MaxStockThreshold), maxStock)
        doUpdate event update

    let setStock event stock =
        let update = Builders<CatalogItemDTO>.Update.Set((fun x -> x.AvailableStock), stock)
        doUpdate event update

    let setOnReorder event onReorder =
        let update = Builders<CatalogItemDTO>.Update.Set((fun x -> x.OnReorder), onReorder)
        doUpdate event update
    
    let deleteCatalogItem (event:EventEnvelope<Event>) =
        let (AggregateId id) = event.AggregateId
        try
            catalogItemsCollection.DeleteOne(fun ci -> ci.Id = id) |> ignore
            ok event
        with ex -> Bad [Error ex.Message :> IError ]

    let loadCatalogItems () = 
        catalogItemsCollection.Find(Builders.Filter.Empty).ToEnumerable()

    let loadCatalogItemById id = 
        catalogItemsCollection.Find(fun ci -> ci.Id = id).ToEnumerable()

    let loadCatalogItemsByDescription description = 
        catalogItemsCollection.Find(fun ci -> ci.Description = description).ToEnumerable()

    let loadCatalogItemsByTypeAndBrand typeId brandId = 
        catalogItemsCollection.Find(fun ci -> ci.CatalogBrand.Id = brandId && ci.CatalogType.Id = typeId).ToEnumerable()

    let loadCatalogItemsByType typeId = 
        catalogItemsCollection.Find(fun ci -> ci.CatalogType.Id = typeId).ToEnumerable()

    let loadCatalogItemsByBrand brandId = 
        catalogItemsCollection.Find(fun ci -> ci.CatalogBrand.Id = brandId).ToEnumerable()

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

    let listResultToOption result =
        match result |> Seq.toList with
        | [] -> Option.None
        | xs -> xs |> Option.Some 

    let getCatalogItemById id = 
        let result = DataAccess.loadCatalogItemById id |> Seq.toList
        match result with
        | [] -> Option.None
        | xs -> 
            xs
            |> Seq.head
            |> Option.Some 

    let getCatalogItemsByDescription = 
        DataAccess.loadCatalogItemsByDescription >> listResultToOption

    let getCatalogItemsByTypeAndBrand typeId brandId =
        DataAccess.loadCatalogItemsByTypeAndBrand typeId brandId 
        |> listResultToOption
        
    let getCatalogItemsByType =
        DataAccess.loadCatalogItemsByType >> listResultToOption
        
    let getCatalogItemsByBrand =
        DataAccess.loadCatalogItemsByBrand >> listResultToOption
        