namespace CatalogUI

open Elmish
open System
open Thoth.Fetch

open CatalogUI
open CatalogUI.AppTypes
open CatalogUI.CatalogApi

module UpdateFn =


  let update (msg: Msg) (state: State) =
      match msg with

      | GetCatalogItem (_, Started) when state.CatalogItem = Deferred.InProgress -> state, Cmd.none
      | GetCatalogItem (id, Started) ->
          {state with CatalogItem = Deferred.InProgress }, Cmd.OfPromise.perform getCatalogItemById id (fun r -> GetCatalogItem (id, Finished r))
      | GetCatalogItem (id, Finished (Ok result)) ->
        { state with CatalogItem = Deferred.Resolved result }, Cmd.none
      // | GetCatalogItem (id, Finished (Error error)) -> ????
      //   { state with CatalogItem = Resolved result }, Cmd.none
      | GetCatalogItems Started when state.CatalogItems = Deferred.InProgress -> state, Cmd.none
      | GetCatalogItems Started ->
          {state with CatalogItems = Deferred.InProgress}, Cmd.OfPromise.perform getCatalogItems () (Finished >> GetCatalogItems)
      | GetCatalogItems (Finished (Ok items)) ->
          { state with CatalogItems = Deferred.Resolved items}, Cmd.none
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
