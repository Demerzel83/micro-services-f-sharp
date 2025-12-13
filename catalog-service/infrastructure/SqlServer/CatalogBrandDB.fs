namespace Microsoft.eShopOnContainers.Services.Catalog.Infrastructure.SqlServer.CatalogBrandDB

open Microsoft.eShopOnContainers.Services.Catalog.Core.Types
open Microsoft.eShopOnContainers.Services.Catalog.Core.CatalogBrandAggregate
open Microsoft.eShopOnContainers.Services.Catalog.Infrastructure.SqlServer.EventsDB
open Dapper
open Microsoft.Data.SqlClient
open System
open TaskResult

module private DataAccess =
    [<Literal>]
    let connectionString = "Server=localhost;Database=Catalog;User=sa;Password=MyVeryStrongPassword123!;TrustServerCertificate=True"


    let loadLastEvent() : TaskResult<int, IError> =
        let query = "SELECT TOP 1 LastEventNumber FROM Dbo.CatalogBrand ORDER BY LastEventNumber DESC"
       
        try
            task {
                use connection = new SqlConnection(connectionString)
                let! nums = connection.QueryAsync<int>(query)  
                let r = nums |> Seq.toList
                return match r with
                        | [] -> Ok 0
                        | _ -> Ok (r.Head)
            }
        with ex -> Error ex.Message :> IError |> Result.Error |> MyTask.rtn
    
    let insertCatalogBrand (event: EventEnvelope<Event>) (catalogItem:CatalogBrandCreation) : TaskResult<EventEnvelope<Event>, IError> =
        let (AggregateId id ) = event.AggregateId

        let query = "INSERT INTO Dbo.CatalogBrand (Id, Brand, LastEventNumber) VALUES (@Id, @Brand, @LastEventNumber)"
        
        try
            task {
                use connection = new SqlConnection(connectionString)
                let parameters = {|Id = id; Brand= catalogItem.Brand; LastEventNumber = event.EventNumber |}

                let! _ = connection.ExecuteAsync(query, parameters)  
                return Ok event
            }
        with ex -> Error ex.Message :> IError |> Result.Error |> MyTask.rtn
    
    let setBrand (event:EventEnvelope<Event>) brand : TaskResult<EventEnvelope<Event>, IError> =
        let (AggregateId id) = event.AggregateId
        let query = "UPDATE Dbo.CatalogBrand SET Brand = @Brand WHERE Id = @Id"
        

        try
            task {
                use connection = new SqlConnection(connectionString)
                let parameters = {|Brand = brand; Id = id|}
                let! _ = connection.ExecuteAsync(query, parameters)  

                return  Ok event
            }
        with ex -> Error ex.Message :> IError |> Result.Error |> MyTask.rtn

    let deleteCatalogBrand (event:EventEnvelope<Event>) : TaskResult<EventEnvelope<Event>, IError> =
        let (AggregateId id) = event.AggregateId
        let query = "DELETE FROM Dbo.CatalogBrand WHERE Id = @Id"
        
        try
            task {
                use connection = new SqlConnection(connectionString)
                let! _ = connection.ExecuteAsync(query, {|Id = id|})  
                return Ok event
            }
        with ex -> Error ex.Message :> IError |> Result.Error |> MyTask.rtn

    let queryAll (parameters:Map<string, obj>) : TaskResult<CatalogBrand list, IError> =
        
        let whereClause =
            parameters
            |> Map.toList
            |> List.map(fun (key, _) -> $"{key} = @{key}")
            |> String.concat " AND "

        let query = 
            if whereClause.Length = 0 then
                "SELECT Id, Brand, LastEven Number FROM Dbo.CatalogBrand WHERE {whereClause}"
            else
                "SELECT Id, Brand, LastEven Number FROM Dbo.CatalogBrand"

        let param = new DynamicParameters()
        parameters |> Map.iter(fun key value -> param.Add($"@{key}", value))
        try
            task {
                use connection = new SqlConnection(connectionString)
                let! results = connection.QueryAsync<CatalogBrand>(query, param)  
                return Ok (results |> Seq.toList)
            }
        with ex ->  Error ex.Message :> IError |> Result.Error |> MyTask.rtn

    let loadCatalogBrands () = queryAll ([] |> Map.ofList)
        
    let loadCatalogBrandById id = queryAll (["Id", id] |> Map.ofList)

    let loadCatalogBrandsByName name = queryAll (["Brand", name] |> Map.ofList)

module Writer = 

    module private Helpers =
        let handler (event: EventEnvelope<Event>) =
            match event.Payload with
            | CatalogBrandCreated catalogBrand -> DataAccess.insertCatalogBrand event catalogBrand
            | BrandUpdated brand -> DataAccess.setBrand event brand
            | CatalogBrandDeleted _ -> DataAccess.deleteCatalogBrand event
            | _ -> event |> TaskResult.rtn

    let handleEvents() = taskResult {
        let! lastEventId = DataAccess.loadLastEvent()
        let! eventTypes = Events.loadTypeEvents (Category "CatalogBrand") lastEventId
        let! eventsHandled = 
                eventTypes
                |> List.map Helpers.handler 
                |> TaskResult.sequenceTaskResult

        return eventsHandled
    }

module Reader =
    
    let getCatalogbrands() = DataAccess.loadCatalogBrands()
    let getCatalogbrandById (id:Guid) = 
        id
        |> DataAccess.loadCatalogBrandById 

    let getCatalogbrandByName (name:string) = 
        name
        |> DataAccess.loadCatalogBrandsByName 