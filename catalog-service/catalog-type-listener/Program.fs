open Microsoft.eShopOnContainers.Services.Catalog.Infrastructure.CatalogTypeCommandHandler
open Microsoft.eShopOnContainers.Services.Catalog.Infrastructure.SqlServer.CatalogTypeDB
open TaskResult
open System.Threading

[<EntryPoint>]
let main argv =
    printfn "[INFO] Catalog Type Listener started"
    let rec messageLoop() = task {
        
        let! result = taskResult {
            let! eventList = CatalogTypeCommandHanlder.processCommandQueue()
        
            eventList |> Seq.iter(printfn "%A") 

            let! eventRes = Writer.handleEvents ()
            eventRes |> Seq.iter(printfn "%A")

            return eventRes        
        }
        match result with
        | Ok _ -> ()
        | Error err -> printfn "[Error] Exception in messag loop: %A" err

        do! Async.Sleep(300)
        return! messageLoop()
    }

    let task = messageLoop()
    task.Result

    Thread.Sleep(Timeout.Infinite)
    0

