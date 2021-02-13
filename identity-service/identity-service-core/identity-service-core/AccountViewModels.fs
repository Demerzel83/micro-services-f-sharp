namespace Microsoft.eShopOnContainers.Services.Identity.Core

module AccountViewModelTypes =
    type ConsentInputModel = {
        Button : string 
        ScopesConsented : string list
        RememberConsent : bool 
        ReturnUrl : string 
    }
    
    type ScopeViewModel = {
        Name : string 
        DisplayName : string
        Description : string 
        Emphasize : bool 
        Required : bool
        Checked : bool 
    }

    type ConsentViewModel = {
        ClientName : string 
        ClientUrl : string
        ClientLogoUrl : string 
        AllowRememberConsent : bool 
        IdentityScopes : ScopeViewModel list
        ResourceScopes : ScopeViewModel list
    }

    type ForgotPasswordViewModel = {
        Email : string
    }

    type LoggedOutViewModel = {
        PostLogoutRedirectUri : string 
        ClientName : string
        SignOutIframeUrl : string 
    }

    type LoginViewModel = {
        Email : string 
        Password : string
        RememberMe : string 
        ReturnUrl : string 
    }

    type LogoutViewModel = {
        LogoutId : string 
    }

    type RegisterViewModel = {
        
        Email : string 
        Password : string 
        ConfirmPassword : string 
        User : ApplicationUserTypes.ApplicationUser
    }

    type ResetPasswordViewModel = {
        Email : string 
        Password : string 
        ConfirmPassword : string 
        Code : string
    }

    type SendCodeViewModel = {
        SelectedProvider : string 
        //Providers : SelectListItem list 
        ReturnUrl : string 
        RememberMe : bool
    }

    type VerifyCodeViewModel = {
        Provider : string 
        Code : string 
        ReturnUrl : string 
        RememberBrowser : bool
        RememberMe : bool
    }