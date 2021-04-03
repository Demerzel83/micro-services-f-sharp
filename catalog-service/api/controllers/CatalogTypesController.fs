namespace Microsoft.eShopOnContainers.Services.Catalog.API

open FSharp.Control.Tasks.V2
open Giraffe
open System
open Chessie.ErrorHandling
open Microsoft.eShopOnContainers.Services.Catalog.Core.CatalogTypeModel
open Microsoft.AspNetCore.Authentication.JwtBearer

open Microsoft.eShopOnContainers.Services.Catalog.Core.CatalogTypeAggregate
open Microsoft.eShopOnContainers.Services.Catalog.Core.Commands
open Microsoft.eShopOnContainers.Services.Catalog.Core.Types
//open Microsoft.eShopOnContainers.Services.Catalog.Infrastructure.SqlServer.CatalogTypeReadModel
//open Microsoft.eShopOnContainers.Services.Catalog.Infrastructure.SqlServer.Commands
open Microsoft.eShopOnContainers.Services.Catalog.Infrastructure.MongoDb.CatalogTypeReadModel
open Microsoft.eShopOnContainers.Services.Catalog.Infrastructure.MongoDb.Commands

module CatalogTypesController =
    let authorize =
        requiresAuthentication (challenge JwtBearerDefaults.AuthenticationScheme)

    let getHandlers () =
        choose [
          GET >=> choose [
            route "/types" >=> authorize  >=>
              authorize >=> fun next context ->
                task {
                      let result = Reader.getCatalogTypes() 

                      return! json result next context
                  }
            
            routef "/types/%s" (fun id ->
              authorize >=> fun next context ->
                  task {
                      let result = ["item1"]

                      return! json result next context
                  }
            )
            routef "/types/%s" (fun name ->
              authorize >=> fun next context ->
                  task {
                      let result = [name]

                      return! json result next context
                  }
            )
          ]
          PUT >=> 
            routef "/types/%s" (fun id ->
                authorize >=> fun next context ->
                    task {
                        let! catalogType = context.BindModelAsync<Model.CatalogTypeModel>()
                        let id = AggregateId (new Guid (id))
                        let versionNumber = AggregateVersion.Irrelevant;
                        let cmd = UpdateType catalogType.Type
                        let envelope = createCommand id (versionNumber, None, None, None) cmd

                        let list = [(catalogTypeQueueName,envelope)]
                        let result = queueCommands (List.ofSeq list)

                        return! 
                            (match result with
                            | Result.Ok _ -> Successful.OK catalogType
                            | Result.Bad _ ->  RequestErrors.badRequest (text "error")) next context
                        })
          DELETE >=> routef "/types/%s" (fun ids ->
            authorize >=> fun next context ->
                task {
                    let id = AggregateId (new Guid(ids))
                    let versionNumber = AggregateVersion.Irrelevant;
                    let cmd =   
                        (new Guid(ids))
                        |> Delete
                    let envelope = createCommand id (versionNumber, None, None, None) cmd

                    let list = [(catalogTypeQueueName,envelope)]
                    let result = queueCommands (List.ofSeq list)

                    return! 
                        (match result with
                        | Result.Ok _ -> Successful.OK id
                        | Result.Bad _ ->  RequestErrors.badRequest (text "error")) next context
                })
          POST >=> route "/types/new" >=> authorize  >=>
                fun next context ->
                    task {
                        let! catalogItemM = context.BindModelAsync<Model.CatalogTypeModel>()
                        let newId = Guid.NewGuid()
                        let id = AggregateId newId
                        let versionNumber = Expected 0;
                        let catalogItemCreation = {
                             Type =  catalogItemM.Type
                        }
                        let cmd = Create catalogItemCreation
                        let envelope = createCommand id (versionNumber, None, None, None) cmd

                        let list = [(catalogTypeQueueName,envelope)]
                        let result = queueCommands (List.ofSeq list)

                        return! 
                            (match result with
                            | Result.Ok _ -> Successful.OK { Id = newId; Type = catalogItemM.Type }
                            | Result.Bad _ ->  RequestErrors.badRequest (text "error")) next context
                    }

        ]