module Microsoft.eShopOnContainers.Services.Identity.API.Model.Login

type LoginViewModel =
    {
        Email : string
        Password : string
    }

type TokenResult =
    {
        Token : string
    }