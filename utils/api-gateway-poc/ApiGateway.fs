module ApiGateway
open Newtonsoft.Json
open Newtonsoft.Json.Serialization
open Suave
open Suave.Operators
open Profile


let JSON v =
    let jsonSerializerSettings = new JsonSerializerSettings()
    jsonSerializerSettings.ContractResolver <- new CamelCasePropertyNamesContractResolver()
    
    let step1 = JsonConvert.SerializeObject(v, jsonSerializerSettings) |> Suave.Successful.OK
    let step2 = Writers.setMimeType "application/json; charset=utf-8"
    let step3 = step1 >=> step2
    step3
    
let getProfile userName (httpContext : Suave.Http.HttpContext) =
       async {
          let! profile = getProfile userName
          match profile with
          | Some p -> return! JSON p httpContext
          | None -> return! Suave.RequestErrors.NOT_FOUND (sprintf "Username %s not found" userName) httpContext
       }