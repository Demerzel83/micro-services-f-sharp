module Http

open FSharp.Data
open FSharp.Control.Reactive

type HttpResponse =
    | Ok of string
    | Error of int

let getResponseAsync url =
    async {
        let! response = 
            Http.AsyncRequest (url, httpMethod = "GET", headers= ["User-Agent", "FshapRx"; "content-type", "application/json; charset=UTF-8"])
                    
        printf "API Request: %s\n" url
        let httpResponse =
            match response.StatusCode, response.Body with
            | 200, Text body -> body |> Ok
            | _ -> response.StatusCode |> Error

        return httpResponse
    }

let asyncResponseToObservable = getResponseAsync >> Observable.ofAsync
