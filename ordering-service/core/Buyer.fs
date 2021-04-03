namespace Microsoft.eShopOnContainers.Services.Ordering.Core

open System

module BuyerTypes =
    type CardType = 
    | Amex
    | Visa
    | MasterCard

    type PaymentMethod = {
        Alias: string
        CardNumber : string
        SecurityNumber : string
        CardHolderName : string
        Expiration : DateTime
        CardType : CardType
    }
    
    type Buyer = {
        IdentityGuid : string
        Name : string
        PaymentMethods : PaymentMethod list
    }