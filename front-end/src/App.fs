module App

open Elmish
open Elmish.React

open CatalogUI
open CatalogUI.AppTypes
open CatalogUI.RenderFn
open CatalogUI.UpdateFn

let init() =
    {
      Loading = false
      CatalogItems = Deferred.HasNotStartedYet
      CatalogItem = Deferred.HasNotStartedYet
    }, Cmd.ofMsg (GetCatalogItems Started)

Program.mkProgram init update render
|> Program.withReactSynchronous "elmish-app"
|> Program.run