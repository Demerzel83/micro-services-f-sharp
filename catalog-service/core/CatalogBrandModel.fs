namespace Microsoft.eShopOnContainers.Services.Catalog.Core.CatalogBrandModel

open Microsoft.eShopOnContainers.Services.Catalog.Core.CatalogBrandAggregate

module Model =

    type CatalogBrandModel = {
        Brand: string
    }

    let FromDto (catalogBrandDTO:CatalogBrand) =
        { Brand = catalogBrandDTO.Brand }
