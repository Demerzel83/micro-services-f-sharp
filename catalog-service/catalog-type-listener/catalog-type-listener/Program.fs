// Learn more about F# at http://fsharp.org

open System
open Chessie.ErrorHandling
open Microsoft.eShopOnContainers.Services.Catalog.CatalogTypeCommandHandler
open Microsoft.eShopOnContainers.Services.Catalog.API

[<EntryPoint>]
let main argv =
    printfn "[INFO] Catalog Type Listener started"
    let rec loop() =
        let eventList = CatalogTypeCommandHanlder.processCommandQueue()
        
        let res =
            match eventList with
            | Ok(r,_ )->
                r
                |> Seq.iter (fun r' -> 
                             match r' with
                             | Ok(r'', _) -> Seq.iter(fun r''' -> printfn "%A" r''') r''
                             | Bad f -> printfn "%A" f)
            | Bad f -> printfn "%A" f

        let eventRes = ReadModel.Writer.handleEvents ()
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