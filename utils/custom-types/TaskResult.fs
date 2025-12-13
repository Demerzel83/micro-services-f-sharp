module TaskResult
open System.Threading.Tasks
open MyResult
open MyTask

type TaskResult<'a, 'Error> = Task<Result<'a, 'Error>>

let rtn x : TaskResult<'a, 'Error> = task {
    return MyResult.retn x
}

let map f (xTaskResult:TaskResult<'a, 'Error>) : TaskResult<'b, 'Error> = task {
    let! xResult = xTaskResult
    return MyResult.map f xResult
}

let bind (f:'a -> TaskResult<'b, 'Error>) (xTaskResult:TaskResult<'a, 'Error>): TaskResult<'b, 'Error> = task {
    let! xResult = xTaskResult
    match xResult with 
    | Ok value -> return! f value
    | Error e -> return Error e
}

let apply (f:TaskResult<'a -> TaskResult<'b, 'Error>, 'Error>) (x:TaskResult<'a, 'Error>): TaskResult<'b, 'Error> = task {
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

/// ('a -> TaskResult<'b>) -> 'a list -> TaskResult<'b list>
let rec traverseTaskResult (f: 'a -> TaskResult<'b, 'Error>) (list:'a list) : TaskResult<'b list, 'Error> =
    
    let cons head tail = head :: tail

    match list with
    | [] -> rtn []
    | x::xs ->  task {
        let! resultCons = rtn cons
        let! resultValue = f x
        let resultCons2 = MyResult.apply resultCons resultValue
        let! resultList = traverseTaskResult f xs
        return MyResult.apply resultCons2 resultList
    }

let sequenceTaskResult (x:TaskResult<'a, 'Error> list) : TaskResult<'a list, 'Error> = 
    traverseTaskResult id x

type TaskResultBuilder() =
    member this.Return x = rtn x
    member this.Bind(x,f) = bind f x

let taskResult = new TaskResultBuilder()