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
      Loading : bool
      CatalogItems : CatalogItem list
      CatalogItem : CatalogItem option
    }

type Msg =
    | FetchingCatalogItems
    | FetchingCatalogItem of Guid
    | CatalogItemsLoaded of CatalogItem list
    | CatalogItemLoaded of CatalogItem option
    | SaveCatalogItem of CatalogItemDTO
    | CatalogItemSaved of CatalogItemDTO
    | SetPrice of Guid
    | PriceSet of Guid

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
  Fetch.fetchAs ("/api/items", decodeCatalogItems)

let getCatalogItemById (id:Guid) =
  Fetch.fetchAs ("/api/items/" + id.ToString(), decodeCatalogItem |> Decode.option)

let getCatalogItemByDescription description =
  Fetch.fetchAs ("/api/items/withdescription/" + description, decodeCatalogItems)

let getCatalogItemByTypeBrand (catalogType:Guid) (brand:Guid) =
  Fetch.fetchAs ("/api/items/type/" + catalogType.ToString() + "/brand/" + brand.ToString(), decodeCatalogItems)

let getCatalogItemByType (catalogType:Guid) =
  Fetch.fetchAs ("/api/brand/all/type/" + catalogType.ToString(), decodeCatalogItems)

let getCatalogItemByBrand (brand:Guid) =
  Fetch.fetchAs ("/api/types/all/brand/" + brand.ToString(), decodeCatalogItems)

// put
let setPrice (id:Guid) price =
  let data = Encode.object [
    "Price", Encode.decimal price
  ]
  Fetch.put ("/api/item/price/" + id.ToString(), data, decoder = decodePrice)

let setPicture (id:Guid) fileName url =
  let data = Encode.object [
      "FileName", Encode.string fileName
      "Uri", Encode.string url
  ]
  Fetch.put ("/api/item/picture/" + id.ToString(), data, decoder = Decode.int)

let setStock (id:Guid) stock =
  let data = Encode.object [
    "Stock", Encode.decimal stock
  ]
  Fetch.put ("/api/item/stock/"+ id.ToString(), data, decoder = Decode.int)

let setStockThreshold (id:Guid) reStock maxStock =
  let data = Encode.object [
    "ReStock", Encode.decimal reStock
    "MaxStock", Encode.decimal maxStock
  ]
  Fetch.put ("/api/item/stockthreshold/" + id.ToString(), data, decoder = Decode.int)

let setReorder (id:Guid) reorder =
  let data = Encode.object [
    "Reoder" , Encode.bool reorder
  ]
  Fetch.put ("/api/item/reorder/" + id.ToString(), data, decoder = Decode.int)

// post
let createCatalogItem catalogItem
  = Fetch.post ("/api/item/new", (encodeCatalogItem catalogItem), decoder = decodeCatalogItemDTO)

// delete
let deleteCatalogItem (id:Guid)
  = Fetch.delete ("/api/item/" + id.ToString(), decoder = Decode.int)

let getRandomCatalogItem () = {
      Name = "CFE"
      Description = "Created from the front-end"
      Type = Guid.Parse "1de512a8-d377-4869-a792-b46bb5125fee"
      Brand = Guid.Parse "b1a357ec-f9fb-42df-aa56-5c2712b84c79"
    }

let init() =
    {
      Loading = true
      CatalogItems = []
      CatalogItem = None
    }, Cmd.ofMsg FetchingCatalogItems

let update (msg: Msg) (state: State) =
    match msg with
    | CatalogItemsLoaded catalogItems ->
      { state with CatalogItems = catalogItems; Loading = false }, Cmd.none
    | CatalogItemLoaded catalogItem ->
      { state with CatalogItem = catalogItem; Loading = false }, Cmd.none
    | FetchingCatalogItems ->
        {state with Loading = true}, Cmd.OfPromise.perform getCatalogItems () CatalogItemsLoaded
    | FetchingCatalogItem id ->
        {state with Loading = true}, Cmd.OfPromise.perform getCatalogItemById id CatalogItemLoaded
    | SaveCatalogItem catalogItemDto ->
        {state with Loading = true}, Cmd.OfPromise.perform createCatalogItem catalogItemDto (fun _ -> FetchingCatalogItems)
    | CatalogItemSaved catalogItem ->
        {state with Loading = false}, Cmd.none
    | SetPrice id ->
        { state with Loading = true}, Cmd.OfPromise.perform (setPrice id) 666m (fun _ -> FetchingCatalogItems)
     | PriceSet id ->
        { state with Loading = false}, Cmd.none


let catalogItemRows (catalogItems) =
  List.map(fun (catalogItem:CatalogItem) ->
    Html.div [
       Html.text catalogItem.Name
       Html.text catalogItem.Description
       Html.text catalogItem.Type.Type
       Html.text catalogItem.Brand.Brand
    ]
  ) catalogItems

let render (state: State) (dispatch: Msg -> unit) =
  if state.Loading then Html.div [ Html.label [ prop.text "Loading ..."]]
  else
  Html.div [
    Html.div [Html.text "Full List"]
    Html.div (catalogItemRows state.CatalogItems)
    Html.br []
    state.CatalogItem
    |> Option.map (fun catalogItem ->
        Html.div [
          Html.div [Html.text "Catlog item by id"]
          Html.div  [
            Html.h3 [ Html.text "Description"]
            Html.div [ Html.text catalogItem.Name ]
          ]
       ]
    )
    |> Option.defaultValue (Html.text "No Item")
    Html.div [Html.text "Action"]
    Html.div [
      Html.button [
        prop.onClick (fun _ -> dispatch (FetchingCatalogItem (Guid.Parse "7ecf255b-0942-49c8-b053-f3ba5b67c58e")))
        prop.text "Fetch Catalog Item By Id"
      ]
      Html.button [
        prop.onClick (fun _ -> dispatch (SaveCatalogItem (getRandomCatalogItem())))
        prop.text "Create new catalog item"
      ]
      Html.button [
        prop.onClick (fun _ -> dispatch (SetPrice (Guid.Parse "7ecf255b-0942-49c8-b053-f3ba5b67c58e")))
        prop.text "Set Price"
      ]

    ]
  ]



Program.mkProgram init update render
|> Program.withReactSynchronous "elmish-app"
|> Program.run