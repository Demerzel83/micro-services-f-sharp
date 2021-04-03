namespace Microsoft.eShopOnContainers.Services.Webhook.Api

module Webhook = 

    open System

    type WebhookData = {
        When : DateTime
        Payload : string
        Type : string
    } 

    type WebhookType = 
        | CatalogItemPriceChange
        | OrderShipped
        | OrderPaid

    type WebhookSubscription = {
        Id : int
        Type : WebhookType
        Date : DateTime
        DestUrl : string
        Token : string
        UserId : string
    } 

