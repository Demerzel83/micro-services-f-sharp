namespace CatalogUI

open System
open Thoth.Fetch
open Thoth.Json

open CatalogUI.CatalogDecoders
open CatalogUI.CatalogEncoders

module CatalogApi =
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