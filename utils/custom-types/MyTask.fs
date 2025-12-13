module MyTask

let rtn x = task { return x }

let map f xTask = 
    task {
        let! x = xTask
        return f x
    }

let bind f xTask =
    task {
        let! x = xTask
        return! f x
    }

let apply fTask xTask =
    task {
        let! x = xTask
        let! f = fTask
        return f x
    }