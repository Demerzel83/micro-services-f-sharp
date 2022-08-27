namespace Microsoft.eShopOnContainers.Services.Catalog.Infrastructure.MongoDb.ReadModel

open FSharp.Data.Sql
open System
open Chessie.ErrorHandling
open MongoDB.Driver
open MongoDB.Bson
//open MongoDB.FSharp

open Microsoft.eShopOnContainers.Services.Catalog.Core.Types
open Microsoft.eShopOnContainers.Services.Catalog.Core.CatalogItemAggregate
open Microsoft.eShopOnContainers.Services.Catalog.Infrastructure.MongoDb
open System.Linq
open Microsoft.FSharp.Control
open System.Threading

type CatalogTypeDTO = {
    Id : Guid
    Type : string
  }

type CatalogBrandDTO = {
      Id : Guid
      Brand : string
    }


type CatalogItemDTO = {
    Id : ObjectId
    Name : string
    Description : string
    Price: decimal
    PictureFileName : string
    PictureUri: string
    //CatalogType : CatalogTypeDTO
    CatalogTypeId: Guid
    //CatalogBrand: CatalogBrandDTO
    CatalogBrandId: Guid
    AvailableStock: decimal
    RestockThreshold: decimal
    MaxStockThreshold: decimal
    OnReorder: bool
    LastEventNumber : int
  }

  
module private DataAccess =
    open MongoDB.Bson
    open BagnoDB

    let collection = "CatalogItems"
    let database = "CatalogItems"
       
    let config = {
        host = "localhost"
        port = 27017
        user = None
        password = None
        authDb = None
    }

    let connection = 
        Connection.host config
        |> Connection.database database
        |> Connection.collection collection

    let loadLastEvent () = 
        try
            let filterGt = Filter.gt (fun (o: CatalogItemDTO) -> o.LastEventNumber ) 0
            let filterOpt = FindOptions<CatalogItemDTO>()
            async {
                let! result = 
                    connection
                    |> Query.filter CancellationToken.None filterOpt filterGt
                return (if result.Count = 0 
                    then ok 0
                    else ok (result.First().LastEventNumber))
            }
        with ex -> async { return (Bad [ Error ex.Message :> IError ]) }
    
    let insertCatalogItem (event: EventEnvelope<Event>) (catalogItem:CatalogItemCreation) =
        let (AggregateId id ) = event.AggregateId
        let newCatalogItem:CatalogItemDTO = {
            Id = ObjectId.Parse(id.ToString())
            Name = catalogItem.Name
            Description = catalogItem.Description
            Price = 0.0m
            PictureFileName = ""
            PictureUri = ""
            CatalogTypeId = catalogItem.CatalogTypeId
            CatalogBrandId = catalogItem.CatalogBrandId
            AvailableStock  = 0.0m
            RestockThreshold  = 0.0m
            MaxStockThreshold = 0.0m
            OnReorder = false
            LastEventNumber = 666 //?
        }
        try
            let options = InsertOneOptions()
            async {
                do! connection
                    |> Query.insertOne CancellationToken.None options newCatalogItem
                return (ok event)
            }
            
        with ex -> async { return Bad [ Error ex.Message :> IError ] }
    
    let doUpdate (event:EventEnvelope<Event>) update =
        let (AggregateId id) = event.AggregateId
        try
            let idFilter = ObjectId.Parse(id.ToString())
            let filterEq = Filter.eq (fun (o: CatalogItemDTO) -> o.Id ) idFilter
            let options = FindOneAndUpdateOptions<CatalogItemDTO>()
            async {
                do! connection
                    |> Query.update CancellationToken.None options update filterEq
                    |> Async.Ignore

                return ok event
            }
        with ex -> 
            async { return Bad [Error ex.Message :> IError ] }

    let setPrice event price =
        let update = Builders<CatalogItemDTO>.Update.Set((fun x -> x.Price), price)
        doUpdate event update

    let setPicture event fileName uri =
        let update = Builders<CatalogItemDTO>.Update.Set((fun x -> x.PictureUri), uri).Set((fun x -> x.PictureFileName), fileName)
        doUpdate event update
      
    let setStockSettings event reStock maxStock =
        let update = Builders<CatalogItemDTO>.Update.Set((fun x -> x.RestockThreshold), reStock).Set((fun x -> x.MaxStockThreshold), maxStock)
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
            async {
                let options = DeleteOptions()
                let idFilter = ObjectId.Parse(id.ToString())
                let filterEq = Filter.eq(fun (o:CatalogItemDTO) -> o.Id) idFilter
                do! connection
                    |> Query.deleteMany CancellationToken.None options filterEq
                    |> Async.Ignore

                return ok event
            }
        with ex -> async { return Bad [Error ex.Message :> IError ] }

    let loadCatalogItems () = 
        connection
        |> Query.getAll CancellationToken.None (FindOptions<CatalogItemDTO>())

    let loadCatalogItemById id = 
        connection
        |> Query.filter CancellationToken.None (FindOptions<CatalogItemDTO>()) (Filter.eq (fun (o:CatalogItemDTO) -> o.Id) id)

    let loadCatalogItemsByDescription description = 
        connection
        |> Query.filter CancellationToken.None (FindOptions<CatalogItemDTO>()) (Filter.eq (fun (o:CatalogItemDTO) -> o.Description) description)

    let loadCatalogItemsByTypeAndBrand typeId brandId = 
        connection
        |> Query.filter CancellationToken.None (FindOptions<CatalogItemDTO>()) ((Filter.eq (fun (o:CatalogItemDTO) -> o.CatalogBrandId) brandId) &&& (Filter.eq (fun (o:CatalogItemDTO) -> o.CatalogTypeId) typeId) )

    let loadCatalogItemsByType typeId = 
        connection
        |> Query.filter CancellationToken.None (FindOptions<CatalogItemDTO>()) (Filter.eq (fun (o:CatalogItemDTO) -> o.CatalogTypeId) typeId) 

    let loadCatalogItemsByBrand brandId = 
        connection
        |> Query.filter CancellationToken.None (FindOptions<CatalogItemDTO>()) (Filter.eq (fun (o:CatalogItemDTO) -> o.CatalogBrandId) brandId)


module Writer = 
    open Microsoft.eShopOnContainers.Services.Catalog.Infrastructure.Utils.ChessieFn

    module private Helpers =
        let handler (event: EventEnvelope<Event>) =
            match event.Payload with
            | CatalogItemCreated catalogItem -> DataAccess.insertCatalogItem event catalogItem
            | PriceChanged price -> DataAccess.setPrice event price
            | PictureChanged (fileName, uri) -> DataAccess.setPicture event fileName uri
            | StockThresholdChanged (reStock, maxStock) -> DataAccess.setStockSettings event reStock maxStock
            | StockChanged stock -> DataAccess.setStock event stock
            | OnReorderSet -> DataAccess.setOnReorder event true
            | NotReorderSet -> DataAccess.setOnReorder event false
            | CatalogItemDeleted _ -> DataAccess.deleteCatalogItem event
            | _ -> async { return Ok(event, [Error "Skipped" :> IError]) }

    let handleEvents() =
        async {
            let! m = DataAccess.loadLastEvent()
            let! result = bind (Events.loadTypeEvents catelogItemCategory) m
            return Seq.map Helpers.handler <!> result
        }

module Reader =
    let getCatalogItems() = DataAccess.loadCatalogItems() 

    let listResultToOption (result:Async<'a System.Collections.Generic.List>) =
        async {
            let! r = result
            return match (r |> List.ofSeq) with
                    | [] -> Option.None
                    | xs -> xs |> Option.Some 
        }

    let getCatalogItemById (id:Guid) = 
        async {
            let idFilter = ObjectId.Parse(id.ToString())
            let! result = DataAccess.loadCatalogItemById idFilter 
            return match (result |> List.ofSeq) with
                    | [] -> Option.None
                    | xs -> 
                        xs
                        |> Seq.head
                        |> Option.Some 
        }

    let getCatalogItemsByDescription = 
        DataAccess.loadCatalogItemsByDescription >> listResultToOption

    let getCatalogItemsByTypeAndBrand typeId brandId =
        DataAccess.loadCatalogItemsByTypeAndBrand typeId brandId 
        |> listResultToOption
        
    let getCatalogItemsByType =
        DataAccess.loadCatalogItemsByType >> listResultToOption
        
    let getCatalogItemsByBrand =
        DataAccess.loadCatalogItemsByBrand >> listResultToOption
        