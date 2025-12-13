open Microsoft.eShopOnContainers.Services.Catalog.Infrastructure.CataloBrandCommandHandler
open Microsoft.eShopOnContainers.Services.Catalog.Infrastructure.SqlServer.CatalogBrandDB
open TaskResult
open System.Threading

[<EntryPoint>]
let main argv =
    printfn "[INFO] Catalog Brand Listener started"
    
    let rec messageLoop() = task {
        
        let! result = taskResult {
            let! eventList = CatalogBrandCommandHanlder.processCommandQueue()
        
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

    