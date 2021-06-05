namespace CustomTypes

module MyReader =
    type Reader<'r, 'a> = Reader of ('r -> 'a)

    let runReader (Reader f) env = f env

    let returnF a = Reader (fun _ -> a)

    // M<'a> -> ('a -> M<'b>) -> M<b'>
    // Reader<'r, 'a> -> ('a -> Reader<'r, 'b>) -> Reader<'r, 'b>
    // Reader ('r -> 'a) -> ('a -> Reader('r -> 'b)) -> Reader('r -> 'b)
    // ('r -> 'a) -> ('a -> ('r -> 'b)) -> ('r -> 'b)
    let bind m k =
        Reader(fun r -> 
            let result1 = runReader m r
            let result2 = k result1
            runReader result2 r)
    // M<'a> -> (a -> b) -> M<'b>
    // ('r -> 'a) -> (a -> b) -> ('r -> 'b)
    let map r f =
        Reader (fun x -> f (runReader r x))
    let ask = Reader id

    type ReaderBuilds() =
        member this.Return(a) = returnF
        member this.Bind(m, k) = bind
        member this.ReturnFrom((a:Reader<'r, 'a>)) = a

    let MyReader = new ReaderBuilds()
module MyReaderTest =            
    open MyReader
    let asks f = MyReader {
        let! r = ask
        return (f r)
    }

    let local f m = Reader(f >> runReader m)

    //let openPage (url:string) = MyReader {
    //    let! (browser:Browser) = ask
    //    return browser.GoTo url }

    //let openPage (url:string) =
    //    Reader (fun (browser:Browser) -> browser.GoTo url)