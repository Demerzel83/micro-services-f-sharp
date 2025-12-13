module AsyncResult
open MyResult
open MyAsync

type AsyncResult<'a, 'Error> = Async<Result<'a, 'Error>>

let retn x : AsyncResult<'a, 'Error> = async { return Ok x }
let doForget (f:'a -> 'b)  (x:AsyncResult<'a, 'Error>): unit = 
    MyResult.map >> MyAsync.map |> ignore
    

let map (f:'a -> 'b)  (x:AsyncResult<'a, 'Error>): AsyncResult<'b, 'Errror> =
    let f2 = MyResult.map >> MyAsync.map
    f2 f x

let bind (f:'a -> AsyncResult<'b, 'Error>) (x:AsyncResult<'a, 'Error>): AsyncResult<'b, 'Error> = async {
    let! result = x
    match result with 
    | Ok value -> return! f value
    | Error e -> return Error e
}
    
    

let apply (f:AsyncResult<'a -> AsyncResult<'b, 'Error>, 'Error>) (x:AsyncResult<'a, 'Error>): AsyncResult<'b, 'Error> = async {
    let! xResult = x
    let! fResult = f
    match xResult, fResult with
    | Ok value, Ok fn -> return! fn value
    | Error eValue, Error eFn -> return Error eValue
    | Error eValue, _ -> return Error eValue
    | _ , Error efn -> return Error efn

}

let (<!>) = map
let (<*>) = apply
let (>>=) = bind

/// ('a -> AsyncResult<'b>) -> 'a list -> AsyncResult<'b list>
let rec traverseAsyncResult (f: 'a -> AsyncResult<'b, 'Error>) (list:'a list) : AsyncResult<'b list, 'Error> =
    
    let cons head tail = head :: tail

    match list with
    | [] -> retn []
    | x::xs ->  async {
        let! resultCons = retn cons
        let! resultValue = f x
        let resultCons2 = MyResult.apply resultCons resultValue
        let! resultList = traverseAsyncResult f xs
        return MyResult.apply resultCons2 resultList
    }

let sequenceAsyncResult (x:AsyncResult<'a, 'Error> list) : AsyncResult<'a list, 'Error> = 
    traverseAsyncResult id x

type AsyncResultBuilder() =
    member this.Return x = retn x
    member this.Bind(x,f) = bind f x

let asyncResult = new AsyncResultBuilder()