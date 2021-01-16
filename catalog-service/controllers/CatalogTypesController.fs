namespace Microsoft.eShopOnContainers.Services.Catalog.API

open FSharp.Control.Tasks.V2
open Giraffe
open Microsoft.eShopOnContainers.Services.Catalog.API.CatalogTypeReadModel
open Microsoft.eShopOnContainers.Services.Catalog.API.CatalogTypeAggregate
open Microsoft.eShopOnContainers.Services.Catalog.API.Commands
open Microsoft.eShopOnContainers.Services.Catalog.SqlServer.Commands
open Types
open System
open Chessie.ErrorHandling
open Microsoft.eShopOnContainers.Services.Catalog.API.CatalogTypeModel

module CatalogTypesController =
    let getHandlers () =
        choose [
          GET >=> choose [
            route "/types" >=>
              fun next context ->
                task {
                      let result = Reader.getCatalogTypes() 

                      return! json result next context
                  }
            
            routef "/types/%s" (fun id ->
              fun next context ->
                  task {
                      let result = ["item1"]

                      return! json result next context
                  }
            )
            routef "/types/%s" (fun name ->
              fun next context ->
                  task {
                      let result = [name]

                      return! json result next context
                  }
            )
          ]
          PUT >=> 
            routef "/types/%s" (fun id ->
                fun next context ->
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
            fun next context ->
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
          POST >=> route "/types/new" >=>
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