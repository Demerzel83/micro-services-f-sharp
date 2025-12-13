module MyResult

let map f xResult =
        match xResult with
        | Ok x ->
            Ok (f x)
        | Error errs ->
            Error errs
    
let retn x = Ok x

let apply fResult xResult =
    match fResult,xResult with
    | Ok f, Ok x ->
        Ok (f x)
    | Error errs, Ok x ->
        Error errs
    | Ok f, Error errs ->
        Error errs
    | Error errs1, Error errs2 ->
        // concat both lists of errors
        // Error (List.concat [errs1; errs2])
        Error errs1
// Signature: Result<('a -> 'b)> -> Result<'a> -> Result<'b>

let bind f xResult =
    match xResult with
    | Ok x ->
        f x
    | Error errs ->
        Error errs
    
// f: 'a -> 'b -> 'c, a: Result<'a, 'error>, b:Rsult<'b, 'error>
let lift2 f a b =
    match a, b with
    | Ok aVal, Ok bVal -> Ok (f aVal bVal)
    | Error e, _ -> Error e
    | _, Error e -> Error e

let (<!>) = map
let (<*>) = apply
let (>>=) = bind


let rec mapResult f list =
    let cons head tail = head :: tail
    match list with
    | [] ->
        retn []
    | head::tail ->
        retn cons <*> (f head) <*> (mapResult f tail)


/// ('a -> Result<'b>) -> 'a list -> Result<'b list>
let rec traverseResult f list =

    // define the applicative functions
    // define a "cons" function
    let cons head tail = head :: tail

    // loop through the list
    match list with
    | [] ->
        // if empty, lift [] to a Result
        retn []
    | head::tail ->
        // otherwise lift the head to a Result using f
        // and cons it with the lifted version of the remaining list
        retn cons <*> (f head) <*> (traverseResult f tail)

let sequenceResult x = traverseResult id x

type ResultBuilder() =
    member this.Return x = retn x
    member this.Bind(x,f) = bind f x

let result = new ResultBuilder()


