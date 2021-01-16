namespace Microsoft.eShopOnContainers.Services.Catalog.API

open FSharp.Control.Tasks.V2
open Giraffe
open Microsoft.eShopOnContainers.Services.Catalog.API.CatalogBrandReadModel
open Microsoft.eShopOnContainers.Services.Catalog.API.CatalogBrandAggregate
open Microsoft.eShopOnContainers.Services.Catalog.API.Commands
open Microsoft.eShopOnContainers.Services.Catalog.SqlServer.Commands
open Microsoft.eShopOnContainers.Services.Catalog.API.CatalogBrandModel
open Types
open System
open Chessie.ErrorHandling

module CatalogBrandsController =
    let processReadRequest result next context =
        task {
            return! 
              match result with
              | None -> RequestErrors.NOT_FOUND (text "Catalog Brand/s not found") next context
              | Some xs -> Successful.OK xs next context
        }

    let getHandlers () =
        choose [
          GET >=> choose [
            route "/brands" >=>
              fun next context ->
                task {
                      let result = Reader.getCatalogbrands() 
                      return! json result next context
                  }
            
            routef "/brands/%s" (fun id ->
              fun next context ->
                  task {
                      let result = Reader.getCatalogbrandById (Guid.Parse id)
                      return! processReadRequest result next context
                  }
            )
            routef "/brands/%s" (fun name ->
              fun next context ->
                  task {
                      let result = Reader.getCatalogbrandByName name
                      return! processReadRequest result next context
                  }
            )
          ]
          PUT >=> 
            routef "/brands/%s" (fun id ->
                fun next context ->
                    task {
                        let! catalogBrand = context.BindModelAsync<Model.CatalogBrandModel>()
                        let id = AggregateId (new Guid (id))
                        let versionNumber = AggregateVersion.Irrelevant;
                        let cmd = UpdateBrand catalogBrand.Brand
                        let envelope = createCommand id (versionNumber, None, None, None) cmd

                        let list = [(catalogBrandQueueName,envelope)]
                        let result = queueCommands (List.ofSeq list)

                        return! 
                            (match result with
                            | Result.Ok _ -> Successful.OK catalogBrand
                            | Result.Bad _ ->  RequestErrors.badRequest (text "error")) next context
                        })
          DELETE >=> routef "/brands/%s" (fun ids ->
            fun next context ->
                task {
                    let id = AggregateId (new Guid(ids))
                    let versionNumber = AggregateVersion.Irrelevant;
                    let cmd =   
                        (new Guid(ids))
                        |> Delete
                    let envelope = createCommand id (versionNumber, None, None, None) cmd

                    let list = [(catalogBrandQueueName,envelope)]
                    let result = queueCommands (List.ofSeq list)

                    return! 
                        (match result with
                        | Result.Ok _ -> Successful.OK id
                        | Result.Bad _ ->  RequestErrors.badRequest (text "error")) next context
                })
          POST >=> route "/brands/new" >=>
                fun next context ->
                    task {
                        let! catalogBrandM = context.BindModelAsync<Model.CatalogBrandModel>()
                        let newId = Guid.NewGuid()
                        let id = AggregateId (newId)
                        let versionNumber = Expected 0;
                        let catalogBrandCreation = {
                             Brand =  catalogBrandM.Brand
                        }
                        let cmd = Create catalogBrandCreation
                        let envelope = createCommand id (versionNumber, None, None, None) cmd

                        let list = [(catalogBrandQueueName,envelope)]
                        let result = queueCommands (List.ofSeq list)

                        return! 
                            (match result with
                            | Result.Ok _ -> Successful.OK { Id = newId; Brand = catalogBrandM.Brand }
                            | Result.Bad _ ->  RequestErrors.badRequest (text "error")) next context
                    }

        ]