namespace Microsoft.eShopOnContainers.Services.Location.API

module LocationsController =
    open FSharp.Control.Tasks.V2
    open Giraffe

    let getHandlers () =
        choose [
          GET >=> choose [
            route "/locations" >=> 
              fun next context ->
                task {
                      return! json "ok" next context
                  }

            routef "/location/%s" (fun id ->
                fun next context ->
                    task {
                        return! json "ok" next context
                    }
            )
          ]
          PUT >=> 
            routef "/location/%s" (fun id ->
                 fun next context ->
                    task {
                        return! json "ok" next context
                        })
          DELETE >=> routef "/location/%s" (fun ids ->
             fun next context ->
                task {
                    return! json "ok" next context
                })
          POST >=> route "/location/new" >=> 
                fun next context ->
                    task {
                        return! json "ok" next context
                    }

        ]
