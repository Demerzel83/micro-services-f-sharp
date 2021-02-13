namespace CatalogUI

open System

module CatalogTypes =
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