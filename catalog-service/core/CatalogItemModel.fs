namespace Microsoft.eShopOnContainers.Services.Catalog.Core.ReadModel

open System
open Microsoft.eShopOnContainers.Services.Catalog.Core.CatalogItemAggregate


module Model =

    type CatalogItemModel = {
        Name : string
        Description: string
        Type: Guid
        Brand: Guid
    }

    type CatalogItemPrice = {
        Price : decimal
    }

    type CatalogItemImage = {
        FileName:string 
        Uri:string
    }

    type CatalogItemStockThreshold = {
        ReStock:decimal 
        MaxStock:decimal
    }

    type CatalogItemOrder = {
        ReStock:decimal 
        MaxStock:decimal
    }

    type CatalogItemStock = {
        Stock:decimal 
    }

    type CatalogItemReorder = {
        Reorder:bool
    }

    let FromDto (catalogItemDTO:CatalogItem) =
        {  Name = catalogItemDTO.Name
           Description = catalogItemDTO.Description
           Type = catalogItemDTO.CatalogType.Id 
           Brand = catalogItemDTO.CatalogBrand.Id }

