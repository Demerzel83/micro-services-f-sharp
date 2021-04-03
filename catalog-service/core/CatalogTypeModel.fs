namespace Microsoft.eShopOnContainers.Services.Catalog.Core.CatalogTypeModel

open Microsoft.eShopOnContainers.Services.Catalog.Core.CatalogTypeAggregate

module Model =

    type CatalogTypeModel = {
        Type: string
    }

    let FromDto (catalogItemDTO:CatalogType) =
        { Type = catalogItemDTO.Type }
