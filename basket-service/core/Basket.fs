namespace Microsoft.eShopOnContainers.Services.Basket.Core

open System

module BasketTypes = 
    type BaskItem = {
        Id : string 
        ProductId : int 
        ProductName  : string 
        UnitPrice : decimal
        OldUnitPrice : decimal 
        Quantity  : int 
        PictureUrl  : string 
    }

    type BasketCheckout = {
        City : string 
        Street : string 
        State  : string 
        Country : string
        ZipCode : string 
        CardNumber  : string 
        CardHolderName  : string 
        CardExpiration  : DateTime 
        CardSecurityNumber  : string 
        CardTypeId  : int 
        Buyer  : string 
        RequestId  : Guid 
    }

    type CustomerBasket = {
        BuyerId : string 
        Items : BaskItem list
    }
