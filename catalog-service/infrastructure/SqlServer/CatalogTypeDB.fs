namespace Microsoft.eShopOnContainers.Services.Catalog.Infrastructure.SqlServer.CatalogTypeDB

open Microsoft.eShopOnContainers.Services.Catalog.Core.Types
open Microsoft.eShopOnContainers.Services.Catalog.Core.CatalogTypeAggregate
open Microsoft.eShopOnContainers.Services.Catalog.Infrastructure.SqlServer.EventsDB
open Microsoft.Data.SqlClient
open Dapper
open TaskResult
open System

module private DataAccess =
    [<Literal>]
    let connectionString = "Server=localhost;Database=Catalog;User=sa;Password=MyVeryStrongPassword123!;TrustServerCertificate=True"

    let loadLastEvent() : TaskResult<int, IError> =
        let query = "SELECT TOP 1 LastEventNumber FROM Dbo.CatalogType ORDER BY LastEventNumber DESC"

        try
            task {
                use connection = new SqlConnection(connectionString)
                let! num = connection.QueryAsync<int>(query)
                let r = num |> Seq.toList
                return match r with
                        | [] -> Ok 0
                        | _ -> Ok (r.Head)
            }
        with ex -> Error ex.Message :> IError  |> Result.Error |> MyTask.rtn
    
    let insertCatalogType (event: EventEnvelope<Event>) (catalogItem:CatalogTypeCreation) : TaskResult<EventEnvelope<Event>, IError> =
        let (AggregateId id ) = event.AggregateId
        let query = "INSERT INTO Dbo.CatalogType (Id, Type, LastEventNumber) VALUES (@Id, @Type, @LastEventNumber)"
        
        let parameters = {|Id = id; Type = catalogItem.Type; LastEventNumber = event.EventNumber|}
        try
            task {
                use connection = new SqlConnection(connectionString)
                let! _ = connection.ExecuteAsync(query, parameters)
                return Ok event
            }
        with ex -> Error ex.Message :> IError |> Result.Error |> MyTask.rtn
    
    let setType (event:EventEnvelope<Event>) typeDescription : TaskResult<EventEnvelope<Event>, IError> =
        let (AggregateId id) = event.AggregateId
        let query = "UDATE Dbo.CatalogType SET Type = @TypeDescription WHERE Id = @Id"
        
        try 
            task {
                use connection = new SqlConnection(connectionString)    
                let parameters = {|TypeDescription = typeDescription; Id = id|}
                let! _ = connection.ExecuteAsync(query, parameters)
                return Ok event
            }
        with ex -> Error ex.Message :> IError |> Result.Error |> MyTask.rtn

    let deleteCatalogType (event:EventEnvelope<Event>) : TaskResult<EventEnvelope<Event>, IError> =
        let (AggregateId id) = event.AggregateId
        let query = "DELETE FROM Dbo.CatalogType WHERE Id = @Id"
        
        try
            task {
                use connection = new SqlConnection(connectionString)
                let! _ = connection.ExecuteAsync(query, {|Id = id|})
                return Ok event
            }
        with ex -> Error ex.Message :> IError |> Result.Error |> MyTask.rtn

    let loadCatalogTypes () : TaskResult<CatalogType list, IError> =
        let query = "SELECT Id, Type FROM Dbo.CatalogType"
        try
            task {
                use connection = new SqlConnection(connectionString)
                let! catalogTypes = connection.QueryAsync<CatalogType>(query)
                return Ok (catalogTypes |> Seq.toList)
            }
        with ex -> Error ex.Message :> IError |> Result.Error |> MyTask.rtn

    let loadCatalogTypeById (id:Guid)  : TaskResult<CatalogType, IError> =
        let query = "SELECT Id, Type FROM Dbo.CatalogType WHERE Id = @Id"
        try
            task {
                use connection = new SqlConnection(connectionString)
                let! catalogType = connection.QueryFirstAsync<CatalogType>(query, {|Id = id|}) 
                return Ok catalogType
            }
        with ex -> Error ex.Message :> IError |> Result.Error |> MyTask.rtn

    let loadCatalogTypeByName (name:string) : TaskResult<CatalogType, IError> =
        let query = "SELECT Id, Type FROM Dbo.CatalogType WHERE Type = @Name"
        try
            task {
                use connection = new SqlConnection(connectionString)
                let! catalogType = connection.QueryFirstAsync<CatalogType>(query, {|Name = name|})
                return Ok catalogType
            }
        with ex -> Error ex.Message :> IError |> Result.Error |> MyTask.rtn
        

module Writer = 
    
    module private Helpers =
        let handler (event: EventEnvelope<Event>) =
            match event.Payload with
            | CatalogTypeCreated catalogType -> DataAccess.insertCatalogType event catalogType
            | TypeUpdated typeDescription -> DataAccess.setType event typeDescription
            | CatalogTypeDeleted _ -> DataAccess.deleteCatalogType event
            | _ -> event |> TaskResult.rtn

    let handleEvents() = taskResult {
            let! eventId = DataAccess.loadLastEvent() 
            let! eventTypes = Events.loadTypeEvents (Category "CatalogType") eventId
            let! eventsHandled = 
                eventTypes
                |> List.map Helpers.handler
                |> TaskResult.sequenceTaskResult

            return eventsHandled
        }

module Reader =
    let getCatalogTypes = DataAccess.loadCatalogTypes
    let getCatalogTypeById = DataAccess.loadCatalogTypeById
    let getCatalogTypeByName = DataAccess.loadCatalogTypeByName