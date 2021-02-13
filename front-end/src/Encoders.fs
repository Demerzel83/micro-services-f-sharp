namespace CatalogUI

open Thoth.Json

open CatalogUI.CatalogTypes

module CatalogEncoders =
  let encodeCatalogItem (catalogItem:CatalogItemDTO)
      = Encode.object
          [
              "Name", Encode.string catalogItem.Name
              "Description", Encode.string catalogItem.Description
              "Type", Encode.guid catalogItem.Type
              "Brand", Encode.guid catalogItem.Brand
          ]