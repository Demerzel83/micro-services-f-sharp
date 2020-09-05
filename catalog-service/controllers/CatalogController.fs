namespace Microsoft.eShopOnContainers.Services.Catalog.API

open FSharp.Control.Tasks.V2
open Giraffe

module CatalogController =
    let getHandlers () =
        choose [
          GET >=> choose [
            route "/items" >=>
              fun next context ->
                task {
                      let result = ["item1"; "item2"]

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
          PUT >=> route "/items" >=>
            fun next context ->
               task {

                    let result = 1

                    return! context.WriteJsonAsync result
                }
          POST >=> route "/items" >=>
            fun next context ->
                task {
                    let result = 1

                    return! context.WriteJsonAsync result
                }
          DELETE >=> routef "%i" (fun id ->
            fun next context ->
              task {
                    let result = 1

                    return! context.WriteJsonAsync result
                })
        ]