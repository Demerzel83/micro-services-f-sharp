namespace Microsoft.eShopOnContainers.Services.Ordering.Api

module HomeController =
    open FSharp.Control.Tasks.V2
    open Giraffe

    let getHandlers () =
        choose [
          GET >=> choose [
            route "/orders" >=> 
              fun next context ->
                task {
                      return! json "ok" next context
                  }

            routef "/order/%s" (fun id ->
                fun next context ->
                    task {
                        return! json "ok" next context
                    }
            )
          ]
          PUT >=> 
            routef "/order/%s" (fun id ->
                 fun next context ->
                    task {
                        return! json "ok" next context
                        })
          DELETE >=> routef "/order/%s" (fun ids ->
             fun next context ->
                task {
                    return! json "ok" next context
                })
          POST >=> route "/order/new" >=> 
                fun next context ->
                    task {
                        return! json "ok" next context
                    }

        ]
