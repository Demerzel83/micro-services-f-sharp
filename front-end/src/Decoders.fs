namespace CatalogUI

open Thoth.Json

open CatalogUI.CatalogTypes

module CatalogDecoders =
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