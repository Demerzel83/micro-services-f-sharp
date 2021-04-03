namespace Microsoft.eShopOnContainers.Services.Marketing.Core

open System

module ModelTypes =
    type Rule = {
        Id : int
        CampaignId : int
        Description : string
    }

    type Rules = 
        | UserProfileRule of Rule
        | PurchaseHistoryRule of Rule
        | UserLocationRule  of Rule  * int

    type Campaign = {
        Id : int
        Name : string
        Description  : string
        From : DateTime
        To : DateTime
        PictureName : string
        PictureUri : string
        DetailsUri : string
        Rules : Rule list
    }

    type Location = {
        LocationId : int
        Code : string
        Description  : string
    }

    type MarketingData = {
        Id : string
        UserId : string
        Locations  : Location list
        UpdateDate : DateTime
    }

    type RuleType = {
        Id : int
        Name : string
    }

    type UserLocationDetails = {
        LocationId : int
        Code : string
        Description : string
    }