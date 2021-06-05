namespace CustomTypes

module MyState =

    type State<'a, 's> = State of ('s -> 'a * 's)

    let runState (State s) initialState = s initialState

    let getState = State (fun s -> (s,s))
    let putState s = State (fun _ -> ((), s))

    let evalState m s = runState m s |> fst
    let execState m s = runState m s |> snd

    let returnF a = State (fun s -> (a, s))

    // M<'a> -> ('a -> M<'b>) -> M<'b>
    // State<'a, 's> -> ('a -> State<'b, 's>) -> State<'b, 's>
    // ('s -> 'a * 's) -> ('a -> ('s -> 'b * 's )) -> ('s -> 'b * 's )
    // ('s -> 'a * 's) -> ('a -> 's -> 'b * 's ) -> ('s -> 'b * 's )
    let bind (m:State<'a,'s>) (f:'a -> State<'b, 's>) =
        State (fun s -> let (a,s') = runState m s in runState (f a) s')

    // M<'a> -> ('a -> 'b) -> M<'b>
    // ('s -> 'a * 's) -> ('a -> 'b ) -> ('s -> 'b * 's )
    let map (m:State<'a,'s>) (f:'a -> 'b) =
        State (fun s -> 
            let (a', s') = runState m s
            let result2 = f a'
            result2, s'
            )
    type MyStateBuilder() =
        member this.Return a = State (fun s -> (a, s))
        member this.Bind(m, k) = 
          State (fun s -> let (a,s') = runState m s in runState (k a) s')
    let MyState = new MyStateBuilder()

module MyStateTest =

    open MyState

    let enqueue a lst = ((), lst @ [a])
    let dequeue (hd::tl) = (hd, tl)

    let workflow initialState =
        let ((), state1) = enqueue 4 initialState
        let (a,state2) = dequeue state1
        let ((), state3) = enqueue (a*3) state2
        (a,state3)

    let enqueueS a = State(fun s -> ((), s @ [a]))
    let dequeueS = State (fun (hd::tl) ->  (hd, tl))

    let workflowS = MyState {
            let! queue = getState
            do! enqueueS 4
            let! hd = dequeueS
            do! enqueueS (hd*3)
            return hd }

    let tick = MyState {
        let! n = getState
        do! putState (n + 1)
        return n }
            
