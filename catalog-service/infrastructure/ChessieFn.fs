namespace Microsoft.eShopOnContainers.Services.Catalog.Infrastructure.Utils

module ChessieFn =
    open Chessie.ErrorHandling

    let map (f:'a -> 'b) m =
        match m with
        | Chessie.ErrorHandling.Ok (x, l) ->  Ok (f x, l)
        | Chessie.ErrorHandling.Bad x -> Bad x

    let bind (f:int -> Async<Result<'a,'c>>) (x:Result<int, 'c>): Async<Result<'a,'c>> =
        async  {
            let x2 = map f x
            let! x5 = match x2 with
                        | Ok (x3,_) -> x3
                        | Bad x4 ->  async { return Bad x4 }
            return x5
        }

    let (>>=) = bind

