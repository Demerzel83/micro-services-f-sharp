namespace Microsoft.eShopOnContainers.Services.Marketing.Core

open System

module DTOTypes =
    type CampaignDTO = {
        Id : int
        Name : string
        Description  : string
        From : DateTime
        To : DateTime
        PictureUri : string
        DetailsUri : string
    }

    type UserLocationDTO = {
        Id : int
        UserId : Guid
        LocationId  : int
        UpdateDate : DateTime
    }

    type UserLocationRuleDTO = {
        Id : int
        LocationId : int
        Description  : string
    }