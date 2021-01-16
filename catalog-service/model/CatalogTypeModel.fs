namespace Microsoft.eShopOnContainers.Services.Catalog.API.CatalogTypeModel

open Microsoft.eShopOnContainers.Services.Catalog.API.CatalogTypeAggregate

module Model =

    type CatalogTypeModel = {
        Type: string
    }

    let FromDto (catalogItemDTO:CatalogType) =
        { Type = catalogItemDTO.Type }
