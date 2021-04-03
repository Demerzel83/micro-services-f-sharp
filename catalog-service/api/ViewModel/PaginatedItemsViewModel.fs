namespace Microsoft.eShopOnContainers.Services.Catalog.API

module ViewModule =
    type PaginatedItemsViewModel<'Entity> = {
        PageIndex: int
        PageSize: int
        Count : uint64
        Data: 'Entity seq
    }


