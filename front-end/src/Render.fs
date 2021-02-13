namespace CatalogUI

open Elmish
open Elmish.React
open Feliz
open System

open CatalogUI.CatalogTypes
open CatalogUI.AppTypes

module RenderFn =
  let getRandomCatalogItem () = {
      Name = "CFE"
      Description = "Created from the front-end"
      Type = Guid.Parse "1de512a8-d377-4869-a792-b46bb5125fee"
      Brand = Guid.Parse "b1a357ec-f9fb-42df-aa56-5c2712b84c79"
    }

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