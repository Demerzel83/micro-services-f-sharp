namespace Microsoft.eShopOnContainers.Services.Catalog.Infrastructure.SqlServer.CatalogDB

open System
open Microsoft.eShopOnContainers.Services.Catalog.Core.Types
open Microsoft.eShopOnContainers.Services.Catalog.Core.CatalogItemAggregate
open Microsoft.eShopOnContainers.Services.Catalog.Infrastructure.SqlServer.EventsDB
open Dapper
open Microsoft.Data.SqlClient
open Newtonsoft.Json
open System.Data
open TaskResult


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
    let connectionString = "Server=localhost;Database=Catalog;User=sa;Password=MyVeryStrongPassword123!;TrustServerCertificate=True"


    let loadLastEvent(): TaskResult<int, IError> =
        let query = "SELECT TOP 1 LastEventNumber FROM Dbo.Catalog ORDER BY LastEventNumber DESC"
        
        try
            task {
                use connection = new SqlConnection(connectionString)
                let! numbers = connection.QueryAsync<int>(query)   
                let r = numbers|> Seq.toList
                return match r  with
                        | [] -> Ok 0
                        | _ -> Ok (r.Head)  
            }
        with ex ->  Error ex.Message :> IError |> Result.Error |> MyTask.rtn
    
    let insertCatalog (event: EventEnvelope<Event>) (catalog:CatalogItemCreation) : TaskResult<EventEnvelope<Event>, IError> =
        let (AggregateId id) = event.AggregateId
        let query = "INSERT INTO Dbo.Catalog (Id, Name, Description, Price, CatalogTypeId, CatalogBrandId, LastEventNumber, AvailableStock, RestockThreshold, MaxStockThreshold, OnReorder )
                    VALUES (@Id, @Name, @Description, 0.0, @CatalogTypeId, @CatalogBrandId, @LastEventNumber, 0.0, 0.0, 0.0, 0)"
        let parameters = {|
            Id = id; Name = catalog.Name; 
            Description = catalog.Description; 
            CatalogTypeId = catalog.CatalogTypeId; 
            CatalogBrandId = catalog.CatalogBrandId;
            LastEventNumber = event.EventNumber
        |}

        try
            task {
                use connection = new SqlConnection(connectionString)
                let! _ = connection.ExecuteAsync(query, parameters)  
                return Ok event
            }
        with ex -> Error ex.Message :> IError |> Result.Error |> MyTask.rtn
    
    let setPrice (event:EventEnvelope<Event>) price : TaskResult<EventEnvelope<Event>, IError> =
        let (AggregateId id) = event.AggregateId
        let query = "UPDATE Dbo.Catalog SET Price = @Price Where Id = @Id"
        let parameters = {|Price = price; Id = id|}

        try
            task {
                use connection = new SqlConnection(connectionString)
                let! _ = connection.ExecuteAsync(query, parameters)  
                return Ok event
            }
        with ex -> Error ex.Message :> IError |> Result.Error |> MyTask.rtn

    let setPicture (event:EventEnvelope<Event>) fileName uri : TaskResult<EventEnvelope<Event>, IError> =
        let query = "UPDATE Dbo.Catalog SET PictureUri = @PictureUri, PictureFileName=@PictureFileName  Where Id = @Id"
        let parameters = {|PictureUri = uri; PictureFileName=fileName; Id = id|}

        try
            task {
                use connection = new SqlConnection(connectionString)
                let! _ = connection.ExecuteAsync(query, parameters)  
                return Ok event
            }
        with ex -> Error ex.Message :> IError |> Result.Error |> MyTask.rtn
      
    let setStockSettings (event:EventEnvelope<Event>) reStock maxStock : TaskResult<EventEnvelope<Event>, IError> =
        let query = "UPDATE Dbo.Catalog SET RestockThreshold = @RestockThreshold, MaxStockThreshold=@MaxStockThreshold  Where Id = @Id"
        let parameters = {|RestockThreshold = reStock; MaxStockThreshold=maxStock; Id = id|}

        try
            task {
                use connection = new SqlConnection(connectionString)
                let! _ = connection.ExecuteAsync(query, parameters)  
                return Ok event
            }
        with ex -> Error ex.Message :> IError |> Result.Error |> MyTask.rtn

    let setStock (event:EventEnvelope<Event>) stock : TaskResult<EventEnvelope<Event>, IError> =
        let query = "UPDATE Dbo.Catalog SET AvailableStock = @AvailableStock  Where Id = @Id"
        let parameters = {|AvailableStock = stock; Id = id|}

        try
            task {
                use connection = new SqlConnection(connectionString)
                let! _ = connection.ExecuteAsync(query, parameters)  
                return Ok event
            }
        with ex -> Error ex.Message :> IError |> Result.Error |> MyTask.rtn

    let setOnReorder (event:EventEnvelope<Event>) (onReorder:bool) : TaskResult<EventEnvelope<Event>, IError> =
        
        let query = "UPDATE Dbo.Catalog SET OnReorder = @OnReorder  Where Id = @Id"
        let parameters = {|OnReorder = onReorder; Id = id|}
        try
            task {
                use connection = new SqlConnection(connectionString)
                let! _ = connection.ExecuteAsync(query, parameters)  
                return Ok event
            }
        with ex -> Error ex.Message :> IError |> Result.Error |> MyTask.rtn
    
    let deleteCatalog (event:EventEnvelope<Event>) : TaskResult<EventEnvelope<Event>, IError> =
        let (AggregateId id) = event.AggregateId
        let query = "DELETE From Dbo.Catalog WHERE Id = @Id"
        let parameters = {|Id = id|}
        
        try
            task {
                use connection = new SqlConnection(connectionString)
                let! _ = connection.ExecuteAsync(query, parameters)  
                return Ok event
            }
        with ex -> Error ex.Message :> IError |> Result.Error |> MyTask.rtn
    
    let mapRowsToRecords (reader: IDataReader): CatalogItem list =
        [
            while reader.Read() do
                yield {
                    Id = reader.GetGuid 0
                    Name = reader.GetString 1
                    Description = reader.GetString 2
                    Price = reader.GetDecimal 3
                    PictureFileName =  reader.GetString 4 |> Option.ofObj
                    PictureUri = reader.GetString 5 |> Option.ofObj
                    CatalogType = reader.GetString 6 |> JsonConvert.DeserializeObject<CatalogType>
                    CatalogBrand = reader.GetString 7 |> JsonConvert.DeserializeObject<CatalogBrand>
                    AvailableStock = reader.GetDecimal 8
                    ReStockThreshold = reader.GetDecimal 9
                    MaxStockThreshold = reader.GetDecimal 10
                    OnReorder = reader.GetBoolean 11
                }
        ]

    let queryCatalogItem() =
        "SELECT 
                        Catalog.Id, 
                        Name, 
                        Description, 
                        Price, 
                        PictureFileName, 
                        PictureUri, 
                        (
                            SELECT CatalogType.Id, CatalogType.Type
                            FOR JSON PATH, WITHOUT_ARRAY_WRAPPER
                        ) AS CatalogType,
                        (
                            SELECT CatalogBrand.Id, CatalogBrand.Brand
                            FOR JSON PATH, WITHOUT_ARRAY_WRAPPER
                        ) AS CatalogBrand,
                        AvailableStock, 
                        ReStockThreshold,
                        MaxStockThreshold,
                        OnReorder
                     FROM Catalog 
                     INNER JOIN CatalogType ON Catalog.CatalogTypeId = CatalogType.Id
                     INNER JOIN CatalogBrand ON Catalog.CatalogBrandId = CatalogBrand.Id "

    let loadCatalogs () = 
        let query = queryCatalogItem()
        task {
            use connection = new SqlConnection(connectionString)
            do! connection.OpenAsync()  
            use! reader = connection.ExecuteReaderAsync(query)   
            return mapRowsToRecords reader 
        }

    let loadCatalogById (id:Guid) = 
        let query = queryCatalogItem() + " WHERE Catalog.Id = @Id"
        task {
            use connection = new SqlConnection(connectionString)
            do! connection.OpenAsync()  
            let parameters = {|Id = id|}
            use! reader = connection.ExecuteReaderAsync(query, parameters)  
            return mapRowsToRecords reader
        }
        

    let loadCatalogsByDescription (description: string) = 
        let query = queryCatalogItem() + " WHERE Catalog.Description = @Description"
        task {
            use connection = new SqlConnection(connectionString)
            do! connection.OpenAsync()  
            let parameters = {|Description = description|}
            use! reader = connection.ExecuteReaderAsync(query, parameters)  
            return mapRowsToRecords reader
        }
        

    let loadCatalogsByTypeAndBrand (typeId:Guid) (brandId:Guid) = 
        let query = queryCatalogItem() + " WHERE CatalogBrandId = @CatalogBrandId AND CatalogTypeId = @CatalogTypeId"
        task {
            use connection = new SqlConnection(connectionString)
            do! connection.OpenAsync()  
            let parameters = {|CatalogBrandId = brandId; CatalogTypeId = typeId|}
            use! reader = connection.ExecuteReaderAsync(query, parameters)  
            return mapRowsToRecords reader
        }
        

    let loadCatalogsByType (typeId:Guid) = 
        let query = queryCatalogItem() + " WHERE CatalogTypeId = @CatalogTypeId"
        task {
            use connection = new SqlConnection(connectionString)
            do! connection.OpenAsync()  
            let parameters = {|CatalogTypeId = typeId|}
            use! reader = connection.ExecuteReaderAsync(query, parameters)  
            return mapRowsToRecords reader
        }
      

    let loadCatalogsByBrand (brandId:Guid) = 
        let query = queryCatalogItem() + " WHERE CatalogBrandId = @CatalogBrandId"
        task {
            use connection = new SqlConnection(connectionString)
            do! connection.OpenAsync()  
            let parameters = {|CatalogBrandId = brandId|}
            use! reader = connection.ExecuteReaderAsync(query, parameters)   
            return mapRowsToRecords reader
        }

module Writer = 
    module private Helpers =
        let handler (event: EventEnvelope<Event>) : TaskResult<EventEnvelope<Event>, IError> =
            match event.Payload with
            | CatalogItemCreated catalog -> DataAccess.insertCatalog event catalog
            | PriceChanged price -> DataAccess.setPrice event price
            | PictureChanged (fileName, uri) -> DataAccess.setPicture event fileName uri
            | StockThresholdChanged (reStock, maxStock) -> DataAccess.setStockSettings event reStock maxStock
            | StockChanged stock -> DataAccess.setStock event stock
            | OnReorderSet -> DataAccess.setOnReorder event true
            | NotReorderSet -> DataAccess.setOnReorder event false
            | CatalogItemDeleted _ -> DataAccess.deleteCatalog event
            | _ -> event |> TaskResult.rtn

    let handleEvents() = taskResult {
        let! lastEventId = DataAccess.loadLastEvent()
        let! eventTypes = Events.loadTypeEvents (Category "CatalogItem") lastEventId
        let! eventsHandled = 
                eventTypes
                |> List.map Helpers.handler
                |> TaskResult.sequenceTaskResult
        return eventsHandled
    }

module Reader =
    let getCatalogs() = DataAccess.loadCatalogs()

    let listResultToOption (result:CatalogItem list) =
        match result |> Seq.toList with
        | [] -> Option.None
        | xs -> xs |> Option.Some 

    let getCatalogById id = 
        task {
            let! result = DataAccess.loadCatalogById id
            return 
                match result with
                | [] -> Option.None
                | xs -> 
                    xs
                    |> Seq.head
                    |> Option.Some 
        }

    let getCatalogsByDescription = 
        DataAccess.loadCatalogsByDescription >> MyTask.map listResultToOption

    let getCatalogsByTypeAndBrand typeId brandId =
        DataAccess.loadCatalogsByTypeAndBrand typeId brandId 
        |> MyTask.map listResultToOption
        
    let getCatalogsByType =
        DataAccess.loadCatalogsByType >> MyTask.map listResultToOption
        
    let getCatalogsByBrand =
        DataAccess.loadCatalogsByBrand >> MyTask.map listResultToOption
        