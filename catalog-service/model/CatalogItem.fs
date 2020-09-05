namespace Microsoft.eShopOnContainers.Services.Catalog.API


module Model =
  type [<CLIMutable>] CatalogType = {
    Id: int
    Type: string
  }

  type [<CLIMutable>] CatalogBrand = {
    Id : int
    Brand : string
  }

  type [<CLIMutable>] CatalogItem = {
    Id : int
    Name : string
    Description : string
    Price: decimal
    PictureFileName : string
    PictureUri: string
    CatalogTypeId : int
    CatalogType : CatalogType
    CatalogBrandId : int
    CatalogBrand: CatalogBrand
    AvailableStock: int
    RestockThreshold: int
    MaxStockThreshold: int
    OnReorder: bool
  }