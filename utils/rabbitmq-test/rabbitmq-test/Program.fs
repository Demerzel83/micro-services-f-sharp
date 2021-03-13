// Learn more about F# at http://fsharp.org

open System
open Microsoft.eShopOnContainers.Utils.Rabbitmq.Messaging
open System.Threading

[<EntryPoint>]
let main argv =
  let host = "localhost"
  let exchange = "eshoponcontainers"
  let routingKey0 = "all.even"
  let routingKey1 = "all.odd"

  async {
    let token = new CancellationTokenSource()
    token.CancelAfter 5000

    seq { 
      (producer host exchange [routingKey0; routingKey1] token); 
      (consumer "0" host exchange routingKey0 token);  
      (consumer "1" host exchange routingKey1 token);
      (consumer "2" host exchange "all.*" token)  
      (consumer "3" host exchange "*.even" token)  
    }
    |> Async.Parallel
    |> Async.RunSynchronously |> ignore
  } |> Async.RunSynchronously
 
  0
