﻿open Chessie.ErrorHandling
open Microsoft.eShopOnContainers.Services.Catalog.Infrastructure.CommandHandler
//open Microsoft.eShopOnContainers.Services.Catalog.Infrastructure.SqlServer.ReadModel
open Microsoft.eShopOnContainers.Services.Catalog.Infrastructure.MongoDb.ReadModel

[<EntryPoint>]
let main argv =
    printfn "[INFO] Catalog Item Listener started"
    let rec loop() =
        let eventList = CatalogItemCommandHanlder.processCommandQueue()
        
        let res =
            match eventList with
            | Ok(r,_ )->
                r
                |> Seq.iter (fun r' -> 
                             match r' with
                             | Ok(r'', _) -> Seq.iter(fun r''' -> printfn "%A" r''') r''
                             | Bad f -> printfn "%A" f)
            | Bad f -> printfn "%A" f

        let eventRes = Writer.handleEvents ()
        let res2 =
            match eventRes with
            | Ok(r,_) ->
                r
                |> Seq.iter(fun r' ->
                            match r' with
                            | Ok(r'',_) -> printfn "%A" r''
                            | Bad f -> printfn "%A" f)
            | Bad f -> printfn "%A" f

        System.Threading.Thread.Sleep(300)
        loop()
    loop()