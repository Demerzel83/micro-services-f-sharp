namespace Microsoft.eShopOnContainers.Services.Catalog.API.CatalogBrandModel

open Microsoft.eShopOnContainers.Services.Catalog.API.CatalogBrandAggregate

module Model =

    type CatalogBrandModel = {
        Brand: string
    }

    let FromDto (catalogBrandDTO:CatalogBrand) =
        { Brand = catalogBrandDTO.Brand }
