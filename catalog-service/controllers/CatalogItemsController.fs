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
open Microsoft.AspNetCore.Authentication.JwtBearer

module CatalogItemsController =
    let processRequest (id:string) (cmd:Command) (versionNumber:AggregateVersion) (model:'T) next context =
        task {
            let id = AggregateId (new Guid(id))
            let envelope = createCommand id (versionNumber, None, None, None) cmd

            let list = [(catalogItemQueueName,envelope)]
            let result = queueCommands (List.ofSeq list)

            return! 
                (match result with
                | Result.Ok _ -> Successful.OK model
                | Result.Bad _ ->  RequestErrors.badRequest (text "error")) next context
        }
    let processReadRequest result next context =
        task {
            return! 
              match result with
              | None -> RequestErrors.NOT_FOUND (text "Contact Type not found") next context
              | Some xs -> Successful.OK xs next context
        }
    let authorize =
        requiresAuthentication (challenge JwtBearerDefaults.AuthenticationScheme)

    let getHandlers () =
        choose [
          GET >=> choose [
            route "/items" >=> authorize >=>
              fun next context ->
                task {
                      let result = Reader.getCatalogItems() 

                      return! json result next context
                  }
            
            routef "/items/%s" (fun id ->
                authorize >=> fun next context ->
                          task {
                              let result = Reader.getCatalogItemById (Guid.Parse id) 
                              return! processReadRequest result next context
                          }
            )
            routef "/items/withdescription/%s" (fun description ->
              authorize >=> fun next context ->
                  task {
                      let result = Reader.getCatalogItemsByDescription description

                      return! processReadRequest result next context
                  }
            )
            routef "/items/type/%s/brand/%s" (fun (catalogTypeId, catalogBrandId) ->
              authorize >=> fun next context ->
                  task {
                      let result = Reader.getCatalogItemsByTypeAndBrand (Guid.Parse catalogTypeId) (Guid.Parse catalogBrandId) 

                      return! processReadRequest result next context
                  }
            )
            routef "/items/type/all/brand/%s" (fun catalogBrandId ->
              authorize >=> fun next context ->
                  task {
                      let result = Reader.getCatalogItemsByBrand (Guid.Parse catalogBrandId) 

                      return! processReadRequest result next context
                  }
            )
            routef "/items/brand/all/type/%s" (fun typeId -> 
                authorize >=> fun next context ->
                    task {
                        let result = Reader.getCatalogItemsByType (Guid.Parse typeId) 
                        
                        return! processReadRequest result next context
                    }
            )
          ]
          PUT >=> 
            routef "/item/price/%s" (fun id ->
                authorize >=> fun next context ->
                    task {
                        let! catalogItemPrice = context.BindModelAsync<Model.CatalogItemPrice>()
                        let cmd = SetPrice catalogItemPrice.Price
                        return! processRequest id cmd AggregateVersion.Irrelevant catalogItemPrice next context 
                        })
            routef "/item/picture/%s" (fun id ->
                authorize >=> fun next context ->
                    task {
                        let! catalogItemImage = context.BindModelAsync<Model.CatalogItemImage>()
                        let cmd = SetPicture (catalogItemImage.FileName, catalogItemImage.Uri)
                        return! processRequest id cmd AggregateVersion.Irrelevant catalogItemImage next context 
                        })
            routef "/item/stock/%s" (fun id -> 
                authorize >=> fun next context ->
                    task {
                        let! catalogItemStock = context.BindModelAsync<Model.CatalogItemStock>()
                        let cmd = SetStock catalogItemStock.Stock
                        return! processRequest id cmd AggregateVersion.Irrelevant catalogItemStock next context
                        })
            routef "/item/stockthreshold/%s" (fun id ->
                authorize >=> fun next context ->
                    task {
                        let! catalogItemStockThreshold = context.BindModelAsync<Model.CatalogItemStockThreshold>()
                        let cmd = UpdateStockThreshold (catalogItemStockThreshold.ReStock, catalogItemStockThreshold.MaxStock)
                        return! processRequest id cmd AggregateVersion.Irrelevant catalogItemStockThreshold next context
                        })
            routef "/item/reorder/%s" (fun id -> 
                authorize >=> fun next context ->
                    task {
                        let! catalogItemReorder = context.BindModelAsync<Model.CatalogItemReorder>()
                        let cmd = if catalogItemReorder.Reorder = true then OnReorder else NotOnReorder
                        return! processRequest id cmd AggregateVersion.Irrelevant catalogItemReorder next context 
                    })
          DELETE >=> routef "/item/%s" (fun ids ->
            authorize >=> fun next context ->
                task {
                    let id = AggregateId (new Guid(ids))
                    let cmd =   
                        (new Guid(ids))
                        |> Delete

                    return! processRequest ids cmd AggregateVersion.Irrelevant id next context 
                    
                })
          POST >=> route "/item/new" >=> authorize  >=>
                fun next context ->
                    task {
                        
                        let! catalogItemM = context.BindModelAsync<Model.CatalogItemModel>()
                        let newId = Guid.NewGuid()
                        let id = AggregateId newId
                        let catalogItemCreation = {
                             Name =  catalogItemM.Name
                             Description = catalogItemM.Description
                             CatalogTypeId = catalogItemM.Type
                             CatalogBrandId = catalogItemM.Brand
                        }
                        let cmd = Create catalogItemCreation
                        return! processRequest (newId.ToString()) cmd (Expected 0) catalogItemM next context 
                    }

        ]

    