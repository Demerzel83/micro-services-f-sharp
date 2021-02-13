namespace CatalogUI

open System
open Thoth.Fetch

open CatalogUI
open CatalogUI.CatalogTypes

module AppTypes =
  type State =
    {
      CatalogItems : Deferred.Deferred<CatalogItem list>
      CatalogItem : Deferred.Deferred<CatalogItem option>
      Loading: bool
    }

  type AsyncOperationStatus<'t> =
    | Started
    | Finished of 't

  type Msg =
    | GetCatalogItem of Guid * AsyncOperationStatus<Result<CatalogItem option, FetchError>>
    | GetCatalogItems of AsyncOperationStatus<Result<CatalogItem list, FetchError>>
    | SaveCatalogItem of CatalogItemDTO * AsyncOperationStatus<Result<CatalogItemDTO, FetchError>>
    | SetPrice of Guid * decimal * AsyncOperationStatus<Result<decimal, FetchError>>