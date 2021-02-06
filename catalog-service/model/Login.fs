module Microsoft.eShopOnContainers.Services.Catalog.API.Model.Login

type LoginViewModel =
    {
        Email : string
        Password : string
    }

type TokenResult =
    {
        Token : string
    }