namespace Microsoft.eShopOnContainers.Services.Catalog.API

open FSharp.Control.Tasks.V2
open Giraffe
open Microsoft.eShopOnContainers.Services.Catalog.API.ReadModel
open Microsoft.eShopOnContainers.Services.Catalog.API.CatalogItemAggregate
open Microsoft.eShopOnContainers.Services.Catalog.API.Commands
open Microsoft.eShopOnContainers.Services.Catalog.SqlServer.Commands
open Types
open System
open Chessie.ErrorHandling

module CatalogController =
    let getHandlers () =
        choose [
          GET >=> choose [
            route "/items" >=>
              fun next context ->
                task {
                      let result = Reader.getCatalogItems() 

                      return! json result next context
                  }
            
            routef "/items/%i" (fun id ->
              fun next context ->
                  task {
                      let result = ["item1"]

                      return! json result next context
                  }
            )
            routef "/items/withname/%s" (fun name ->
              fun next context ->
                  task {
                      let result = [name]

                      return! json result next context
                  }
            )
            routef "/items/type/%i/brand/%i" (fun (catalogTypeId, catalogBrandId) ->
              fun next context ->
                  task {
                      let result = [catalogTypeId; catalogBrandId]

                      return! json result next context
                  }
            )
            routef "/items/type/all/brand/%i" (fun catalogBrandId ->
              fun next context ->
                  task {
                      let result = [catalogBrandId]

                      return! json result next context
                  }
            )
            route "/catalogtypes" >=> fun next context ->
                task {
                    let result = ["test"]

                    return! json result next context
                }

            route "/catalogbrands" >=>
            fun next context ->
                task {
                    let result = ["test"]

                    return! json result next context
                }
          ]
          PUT >=> 
            routef "/item/price/%s" (fun id ->
                fun next context ->
                    task {
                        let! catalogItemPrice = context.BindModelAsync<Model.CatalogItemPrice>()
                        let id = AggregateId (new Guid (id))
                        let versionNumber = AggregateVersion.Irrelevant;
                        let cmd = SetPrice catalogItemPrice.Price
                        let envelope = createCommand id (versionNumber, None, None, None) cmd

                        let list = [(catalogItemQueueName,envelope)]
                        let result = queueCommands (List.ofSeq list)

                        return! 
                            (match result with
                            | Result.Ok _ -> Successful.OK catalogItemPrice
                            | Result.Bad _ ->  RequestErrors.badRequest (text "error")) next context
                        })
            routef "/item/picture/%s" (fun id ->
                fun next context ->
                    task {
                        let! catalogItemImage = context.BindModelAsync<Model.CatalogItemImage>()
    
                        let id = AggregateId (new Guid(id))
                        let versionNumber = AggregateVersion.Irrelevant;
                        let cmd = SetPicture (catalogItemImage.FileName, catalogItemImage.Uri)
                        let envelope = createCommand id (versionNumber, None, None, None) cmd

                        let list = [(catalogItemQueueName,envelope)]
                        let result = queueCommands (List.ofSeq list)

                        return! 
                            (match result with
                            | Result.Ok _ -> Successful.OK catalogItemImage
                            | Result.Bad _ ->  RequestErrors.badRequest (text "error")) next context
                        })
            routef "/item/stock/%s" (fun id -> 
                fun next context ->
                    task {
                        let! catalogItemStock = context.BindModelAsync<Model.CatalogItemStock>()
    
                        let id = AggregateId (new Guid(id))
                        let versionNumber = AggregateVersion.Irrelevant;
                        let cmd = SetStock catalogItemStock.Stock
                        let envelope = createCommand id (versionNumber, None, None, None) cmd

                        let list = [(catalogItemQueueName,envelope)]
                        let result = queueCommands (List.ofSeq list)

                        return! 
                            (match result with
                            | Result.Ok _ -> Successful.OK catalogItemStock
                            | Result.Bad _ ->  RequestErrors.badRequest (text "error")) next context
                        })
            routef "/item/stockthreshold/%s" (fun id ->
                fun next context ->
                    task {
                        let! catalogItemStockThreshold = context.BindModelAsync<Model.CatalogItemStockThreshold>()
                        
                        let id = AggregateId (new Guid(id))
                        let versionNumber = AggregateVersion.Irrelevant;
                        let cmd = UpdateStockThreshold (catalogItemStockThreshold.ReStock, catalogItemStockThreshold.MaxStock)
                        let envelope = createCommand id (versionNumber, None, None, None) cmd

                        let list = [(catalogItemQueueName,envelope)]
                        let result = queueCommands (List.ofSeq list)

                        return! 
                            (match result with
                            | Result.Ok _ -> Successful.OK catalogItemStockThreshold
                            | Result.Bad _ ->  RequestErrors.badRequest (text "error")) next context
                        })
            routef "/item/reorder/%s" (fun id -> 
                fun next context ->
                    task {
                        let! catalogItemReorder = context.BindModelAsync<Model.CatalogItemReorder>()
                        
                        let id = AggregateId (new Guid(id))
                        let versionNumber = AggregateVersion.Irrelevant;
                        let cmd = if catalogItemReorder.Reorder = true then OnReorder else NotOnReorder
                        let envelope = createCommand id (versionNumber, None, None, None) cmd

                        let list = [(catalogItemQueueName,envelope)]
                        let result = queueCommands (List.ofSeq list)

                        return! 
                            (match result with
                            | Result.Ok _ -> Successful.OK catalogItemReorder
                            | Result.Bad _ ->  RequestErrors.badRequest (text "error")) next context
                    })
          DELETE >=> routef "/item/%s" (fun ids ->
            fun next context ->
                task {
                    let id = AggregateId (new Guid(ids))
                    let versionNumber = AggregateVersion.Irrelevant;
                    let cmd =   
                        (new Guid(ids))
                        |> Delete
                    let envelope = createCommand id (versionNumber, None, None, None) cmd

                    let list = [(catalogItemQueueName,envelope)]
                    let result = queueCommands (List.ofSeq list)

                    return! 
                        (match result with
                        | Result.Ok _ -> Successful.OK id
                        | Result.Bad _ ->  RequestErrors.badRequest (text "error")) next context
                })
          POST >=> route "/item/new" >=>
                fun next context ->
                    task {
                        let! catalogItemM = context.BindModelAsync<Model.CatalogItemModel>()
                        
                        let id = AggregateId (Guid.NewGuid())
                        let versionNumber = Expected 0;
                        let catalogItemCreation = {
                             Name =  catalogItemM.Name
                             Description = catalogItemM.Description
                             CatalogTypeId = catalogItemM.Type
                             CatalogBrandId = catalogItemM.Brand
                        }
                        let cmd = Create catalogItemCreation
                        let envelope = createCommand id (versionNumber, None, None, None) cmd

                        let list = [(catalogItemQueueName,envelope)]
                        let result = queueCommands (List.ofSeq list)

                        return! 
                            (match result with
                            | Result.Ok _ -> Successful.OK catalogItemM
                            | Result.Bad _ ->  RequestErrors.badRequest (text "error")) next context
                    }

        ]