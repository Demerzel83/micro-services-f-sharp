namespace Microsoft.eShopOnContainers.Services.Catalog.API

module APISettings =
  type [<CLIMutable>] CatalogSettings = {
    PicBaseUrl : string
    EventBusConnection : string
    UseCustomizationData : bool
    AzureStorageEnabled : bool
  }