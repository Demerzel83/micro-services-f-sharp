namespace Microsoft.eShopOnContainers.Services.Catalog.API

open Microsoft.AspNetCore.Mvc
open FSharp.Control.Tasks.V2
open Giraffe
open System
open Microsoft.Extensions.Logging
open Microsoft.eShopOnContainers.Services.Catalog.Core.CatalogTypeModel
// open Microsoft.AspNetCore.Authentication.JwtBearer

open Microsoft.eShopOnContainers.Services.Catalog.Core.CatalogTypeAggregate
open Microsoft.eShopOnContainers.Services.Catalog.Core.Commands
open Microsoft.eShopOnContainers.Services.Catalog.Core.Types
open Microsoft.eShopOnContainers.Services.Catalog.Infrastructure.SqlServer.CatalogTypeDB
open Microsoft.eShopOnContainers.Services.Catalog.Infrastructure.SqlServer.EventsDB.Commands
open TaskResult

module CatalogTypesController =
    //let authorize =
    //    requiresAuthentication (challenge JwtBearerDefaults.AuthenticationScheme)
    let parseResult (result:Result<'a, IError>) = 
         match result with
                | Ok list ->  json list
                | Result.Error e -> json e

    let getHandlers () =
        choose [
          GET >=> choose [
            route "/types" >=>
              fun next context ->
                task {
                      let! result = Reader.getCatalogTypes() 
                      return! (parseResult result) next context
                  }
            
            routef "/types/%s" (fun id ->
              fun next context ->
                  task {
                      let! result = Reader.getCatalogTypeById(Guid.Parse(id))

                      return! (parseResult result) next context
                  }
            )
            routef "/types/name/%s" (fun name ->
              fun next context ->
                  task {
                      let! result = Reader.getCatalogTypeByName name
                      return! (parseResult result) next context
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
                        let! result = queueCommands (List.ofSeq list)

                        return! 
                            (match result with
                            | Result.Ok _ -> Successful.OK catalogType
                            | Result.Error _ ->  RequestErrors.badRequest (text "error")) next context
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
                    let! result = queueCommands (List.ofSeq list)

                    return! 
                        (match result with
                        | Result.Ok _ -> Successful.OK id
                        | Result.Error _ ->  RequestErrors.badRequest (text "error")) next context
                })
          POST >=> route "/types/new" >=>
                fun next context ->
                    task {
                        try
                            let! catalogItemM = context.BindJsonAsync<Model.CatalogTypeModel>()
                            let newId = Guid.NewGuid()
                            let id = AggregateId newId
                            let versionNumber = Expected 0;
                            let catalogItemCreation = {
                                 Type =  catalogItemM.Type
                            }
                            let cmd = Create catalogItemCreation
                            let envelope = createCommand id (versionNumber, None, None, None) cmd

                            let list = [(catalogTypeQueueName,envelope)]
                            let! result = queueCommands (List.ofSeq list)

                            return! 
                                (match result with
                                | Result.Ok _ -> Successful.OK { Id = newId; Type = catalogItemM.Type }
                                | Result.Error _ ->  RequestErrors.badRequest (text "error")) next context
                        with
                        | ex -> 
                            printfn "Error processing request %A" ex
                            return! (RequestErrors.badRequest (text (sprintf "Error: %s" ex.Message))) next context
                    }

        ]