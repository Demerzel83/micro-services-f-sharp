namespace Microsoft.eShopOnContainers.Services.Basket.API

module BasketController =
    open FSharp.Control.Tasks.V2
    open Giraffe

    let getHandlers () =
        choose [
          GET >=> choose [
            route "/baskets" >=> 
              fun next context ->
                task {
                      return! json "ok" next context
                  }
    
            routef "/baskets/%s" (fun id ->
                fun next context ->
                    task {
                        return! json "ok" next context
                    }
            )
          ]
          PUT >=> 
            routef "/basket/%s" (fun id ->
                 fun next context ->
                    task {
                        return! json "ok" next context
                        })
          DELETE >=> routef "/basket/%s" (fun ids ->
             fun next context ->
                task {
                    return! json "ok" next context
                })
          POST >=> route "/basket/new" >=> 
                fun next context ->
                    task {
                        return! json "ok" next context
                    }

        ]