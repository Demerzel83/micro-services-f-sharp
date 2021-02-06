module App

open Elmish
open Elmish.React
open Feliz
open System
open Thoth.Fetch
open Thoth.Json

type Deferred<'t> =
  | HasNotStartedYet
  | InProgress
  | Resolved of 't

type AsyncOperationStatus<'t> =
  | Started
  | Finished of 't

type CatalogType = {
  Id : Guid
  Type : string
}

type CatalogBrand = {
  Id : Guid
  Brand : string
}

type CatalogItem = {
    Name : string
    Description: string
    Type: CatalogType
    Brand: CatalogBrand
}

type CatalogItemDTO = {
    Name : string
    Description: string
    Type: Guid
    Brand: Guid
}

type State =
    {
      CatalogItems : Deferred<CatalogItem list>
      CatalogItem : Deferred<CatalogItem option>
      Loading: bool
    }

type Msg =
    | GetCatalogItem of Guid * AsyncOperationStatus<Result<CatalogItem option, FetchError>>
    | GetCatalogItems of AsyncOperationStatus<Result<CatalogItem list, FetchError>>
    | SaveCatalogItem of CatalogItemDTO * AsyncOperationStatus<Result<CatalogItemDTO, FetchError>>
    | SetPrice of Guid * decimal * AsyncOperationStatus<Result<decimal, FetchError>>

let encodeCatalogItem (catalogItem:CatalogItemDTO)
    = Encode.object
        [
            "Name", Encode.string catalogItem.Name
            "Description", Encode.string catalogItem.Description
            "Type", Encode.guid catalogItem.Type
            "Brand", Encode.guid catalogItem.Brand
        ]

let decodeCatalogType : Decoder<CatalogType> =
  Decode.object (fun x ->
    { Id = x.Required.Field "id" Decode.guid
      Type = x.Required.Field "type" Decode.string }
  )

let decodeCatalogBrand : Decoder<CatalogBrand> =
  Decode.object (fun x ->
    { Id = x.Required.Field "id" Decode.guid
      Brand = x.Required.Field "brand" Decode.string }
  )

let decodeCatalogItem : Decoder<CatalogItem>
  = Decode.object(fun x ->
        { Name = x.Required.Field "name" Decode.string
          Description = x.Required.Field "description" Decode.string
          Type = x.Required.Field "catalogType" decodeCatalogType
          Brand = x.Required.Field "catalogBrand" decodeCatalogBrand }
      )
let decodeCatalogItemDTO : Decoder<CatalogItemDTO>
  = Decode.object(fun x ->
        { Name = x.Required.Field "name" Decode.string
          Description = x.Required.Field "description" Decode.string
          Type = x.Required.Field "type" Decode.guid
          Brand = x.Required.Field "brand" Decode.guid }
      )

let decodePrice : Decoder<decimal>
  = Decode.object(fun x -> x.Required.Field "price" Decode.decimal)

let decodeCatalogItems : Decoder<CatalogItem list>
    = decodeCatalogItem |> Decode.list


// get
let getCatalogItems () =
  Fetch.tryFetchAs ("/api/items", decodeCatalogItems)

let getCatalogItemById (id:Guid) =
  Fetch.tryFetchAs ("/api/items/" + id.ToString(), decodeCatalogItem |> Decode.option)

let getCatalogItemByDescription description =
  Fetch.tryFetchAs ("/api/items/withdescription/" + description, decodeCatalogItems)

let getCatalogItemByTypeBrand (catalogType:Guid) (brand:Guid) =
  Fetch.tryFetchAs ("/api/items/type/" + catalogType.ToString() + "/brand/" + brand.ToString(), decodeCatalogItems)

let getCatalogItemByType (catalogType:Guid) =
  Fetch.tryFetchAs ("/api/brand/all/type/" + catalogType.ToString(), decodeCatalogItems)

let getCatalogItemByBrand (brand:Guid) =
  Fetch.tryFetchAs ("/api/types/all/brand/" + brand.ToString(), decodeCatalogItems)

// put
let setPrice (id:Guid) price =
  let data = Encode.object [
    "Price", Encode.decimal price
  ]
  Fetch.tryPut ("/api/item/price/" + id.ToString(), data, decoder = decodePrice)

let setPicture (id:Guid) fileName url =
  let data = Encode.object [
      "FileName", Encode.string fileName
      "Uri", Encode.string url
  ]
  Fetch.tryPut ("/api/item/picture/" + id.ToString(), data, decoder = Decode.int)

let setStock (id:Guid) stock =
  let data = Encode.object [
    "Stock", Encode.decimal stock
  ]
  Fetch.tryPut ("/api/item/stock/"+ id.ToString(), data, decoder = Decode.int)

let setStockThreshold (id:Guid) reStock maxStock =
  let data = Encode.object [
    "ReStock", Encode.decimal reStock
    "MaxStock", Encode.decimal maxStock
  ]
  Fetch.tryPut ("/api/item/stockthreshold/" + id.ToString(), data, decoder = Decode.int)

let setReorder (id:Guid) reorder =
  let data = Encode.object [
    "Reoder" , Encode.bool reorder
  ]
  Fetch.tryPut ("/api/item/reorder/" + id.ToString(), data, decoder = Decode.int)

// post
let createCatalogItem catalogItem
  = Fetch.tryPost ("/api/item/new", (encodeCatalogItem catalogItem), decoder = decodeCatalogItemDTO)

// delete
let deleteCatalogItem (id:Guid)
  = Fetch.tryDelete ("/api/item/" + id.ToString(), decoder = Decode.int)

let getRandomCatalogItem () = {
      Name = "CFE"
      Description = "Created from the front-end"
      Type = Guid.Parse "1de512a8-d377-4869-a792-b46bb5125fee"
      Brand = Guid.Parse "b1a357ec-f9fb-42df-aa56-5c2712b84c79"
    }

let init() =
    {
      Loading = false
      CatalogItems = HasNotStartedYet
      CatalogItem = HasNotStartedYet
    }, Cmd.ofMsg (GetCatalogItems Started)

let update (msg: Msg) (state: State) =
    match msg with

    | GetCatalogItem (_, Started) when state.CatalogItem = InProgress -> state, Cmd.none
    | GetCatalogItem (id, Started) ->
        {state with CatalogItem = InProgress }, Cmd.OfPromise.perform getCatalogItemById id (fun r -> GetCatalogItem (id, Finished r))
    | GetCatalogItem (id, Finished (Ok result)) ->
      { state with CatalogItem = Resolved result }, Cmd.none
    // | GetCatalogItem (id, Finished (Error error)) -> ????
    //   { state with CatalogItem = Resolved result }, Cmd.none
    | GetCatalogItems Started when state.CatalogItems = InProgress -> state, Cmd.none
    | GetCatalogItems Started ->
        {state with CatalogItems = InProgress}, Cmd.OfPromise.perform getCatalogItems () (Finished >> GetCatalogItems)
    | GetCatalogItems (Finished (Ok items)) ->
        { state with CatalogItems = Resolved items}, Cmd.none
    // | GetCatalogItems (Finished (Error items)) ->  ???
    //     { state with CatalogItems = Resolved items}, Cmd.none
    // inprogress
    | SaveCatalogItem (catalogItemDto, Started) ->
        {state with Loading = true}, Cmd.OfPromise.perform createCatalogItem catalogItemDto (fun r -> (SaveCatalogItem (catalogItemDto, Finished r)))
    | SaveCatalogItem (catalogItem, Finished (Ok id)) ->
        {state with Loading = false}, Cmd.ofMsg (GetCatalogItems Started)
    // | SaveCatalogItem (catalogItem, Finished (Error id)) ->
    //     {state with Loading = false}, Cmd.none
    // inprogress
    | SetPrice (id, price, Started) ->
        { state with Loading = true}, Cmd.OfPromise.perform (setPrice id) price (fun r -> SetPrice (id, price, Finished r))
     | SetPrice (id, price, Finished (Ok r)) ->
        { state with Loading = false}, Cmd.ofMsg (GetCatalogItems Started)


let catalogItemRows (catalogItems) =
  List.map(fun (catalogItem:CatalogItem) ->
    Html.div [
       Html.text catalogItem.Name
       Html.text catalogItem.Description
       Html.text catalogItem.Type.Type
       Html.text catalogItem.Brand.Brand
    ]
  ) catalogItems

let showCatalogItems catalogItems = Html.div [
        Html.div [Html.text "Full List"]
        Html.div (catalogItemRows catalogItems)
  ]
let showCatalogItem catalogItem =
        catalogItem
        |> Option.map (fun (catalogItem:CatalogItem) ->
            Html.div [
              Html.div [Html.text "Catlog item by id"]
              Html.div  [
                Html.h3 [ Html.text "Description"]
                Html.div [ Html.text catalogItem.Name ]
              ]
           ]
        )
        |> Option.defaultValue (Html.text "No Item")

let actions dispatch =
    Html.div[
        Html.div [Html.text "Action"]
        Html.div [
          Html.button [
            prop.onClick (fun _ -> dispatch (GetCatalogItem ((Guid.Parse "7ecf255b-0942-49c8-b053-f3ba5b67c58e"), Started)))
            prop.text "Fetch Catalog Item By Id"
          ]
          Html.button [
            prop.onClick (fun _ -> dispatch (SaveCatalogItem (getRandomCatalogItem(), Started)))
            prop.text "Create new catalog item"
          ]
          Html.button [
            prop.onClick (fun _ -> dispatch (SetPrice ((Guid.Parse "7ecf255b-0942-49c8-b053-f3ba5b67c58e"), 666m, Started)))
            prop.text "Set Price"
          ]
        ]

      ]
let render (state: State) (dispatch: Msg -> unit) =
  let body = (match (state.CatalogItem, state.CatalogItems, state.Loading) with
    | (InProgress, _, _) | (_, InProgress, _) | (_,_, true) -> Html.div [ Html.label [ prop.text "Loading ..."]]
    | (HasNotStartedYet, HasNotStartedYet, _) -> Html.div [ Html.label [ prop.text "Nothing is loaded"]]
    | (Resolved catalogItem, Resolved catalogItems, false) ->
        Html.div [
          showCatalogItems catalogItems
          showCatalogItem catalogItem
        ]
    | (Resolved catalogItem, _, false) -> showCatalogItem catalogItem
    | (_, Resolved catalogItems, false) -> showCatalogItems catalogItems
    | _ -> Html.div [ Html.label [ prop.text "Default"]])

  Html.div[
     body
     actions dispatch
   ]



Program.mkProgram init update render
|> Program.withReactSynchronous "elmish-app"
|> Program.run