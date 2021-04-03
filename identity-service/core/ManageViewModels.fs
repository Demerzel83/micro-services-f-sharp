namespace Microsoft.eShopOnContainers.Services.Identity.Core

module ManageViewModelsTypes =
    type AddPhoneNumberViewModel = {
        PhoneNumber : string 
    }
    
    type ChangePasswordViewModel = {
        OldPassword : string 
        NewPassword : string 
        ConfirmPassword : string 
    }

    type ConfigureTwoFactorViewModel = {
        SelectedProvider : string 
        //Providers : SelectListItem list 
    }

    type FactorViewModel = {
        Purpose : string 
    }

    type IndexViewModel = {
        HasPassword : bool
        //Logins : UserLoginInfo list
        PhoneNumber : string
        TwoFactor : bool
        BrowserRemembered : bool
    }

    type SetPasswordViewModel = {
        NewPassword : string
        ConfirmPassword : string
    }

    type VerifyPhoneNumberViewModel = {
        Code : string
        PhoneNumber : string
    }