namespace Microsoft.eShopOnContainers.Services.Catalog.API

open FSharp.Control.Tasks.V2
open Giraffe
open Microsoft.eShopOnContainers.Services.Catalog.API.CatalogItemAggregate
open Microsoft.eShopOnContainers.Services.Catalog.API.Commands
open Microsoft.eShopOnContainers.Services.Catalog.SqlServer.Commands
open Types
open System
open Chessie.ErrorHandling
open Microsoft.eShopOnContainers.Services.Catalog.API.Model
open Microsoft.IdentityModel.Tokens
open System.IdentityModel.Tokens.Jwt
open System.Security.Claims
open System.Text
open Microsoft.eShopOnContainers.Services.Catalog.API.Model.Login

module LoginController =
    let secret = "spadR2dre#u-ruBrE@TepA&*Uf@U"
    let generateToken email =
        let claims = [|
            Claim(JwtRegisteredClaimNames.Sub, email);
            Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()) |]
    
        let expires = Nullable(DateTime.UtcNow.AddHours(1.0))
        let notBefore = Nullable(DateTime.UtcNow)
        let securityKey = SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret))
        let signingCredentials = SigningCredentials(key = securityKey, algorithm = SecurityAlgorithms.HmacSha256)
    
        let token =
            JwtSecurityToken(
                issuer = "jwtwebapp.net",
                audience = "jwtwebapp.net",
                claims = claims,
                expires = expires,
                notBefore = notBefore,
                signingCredentials = signingCredentials)
    
        let tokenResult:TokenResult = {
            Token = JwtSecurityTokenHandler().WriteToken(token)
        }
    
        tokenResult

    let getHandlers () =
        choose [
          POST >=> route "/login" >=>
                fun next context ->
                    task {
                        let! loginModel = context.BindModelAsync<Login.LoginViewModel>()
                        let tokenResult = generateToken loginModel.Email
                        
                        return! json tokenResult next context
                    }

        ]

    