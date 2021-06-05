open Suave
open ApiGateway

[<EntryPoint>]
let main argv =
    let webpart = Suave.Filters.pathScan "/api/profile/%s" getProfile
    startWebServer defaultConfig webpart
    0 // return an integer exit code