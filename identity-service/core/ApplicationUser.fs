namespace Microsoft.eShopOnContainers.Services.Identity.Core

module ApplicationUserTypes =
    type ApplicationUser = {
        CardNumber : string 
        SecurityNumber : string 
        // [RegularExpression(@"(0[1-9]|1[0-2])\/[0-9]{2}", ErrorMessage = "Expiration should match a valid MM/YY value")]
        Expiration : string 
        CardHolderName : string 
        CardType : int 
        Street  : string 
        City : string 
        State : string 
        Country : string 
        ZipCode : string 
        Name : string 
        LastName : string 
    }
