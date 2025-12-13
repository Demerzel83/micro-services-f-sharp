module MyAsync


let rtn x = async { return x }

let map f xASync = 
    async {
        let! x = xASync
        return f x
    }

let bind f xAsync =
    async {
        let! x = xAsync 
        let! r = f x
        return r
    }

let apply fAsync xAsync =
    async {
        let! x = xAsync
        let! f = fAsync
        return f x
    }
