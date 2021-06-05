module Profile 
open GitHub
open Http
open ObservableExtensions
open System.Reactive.Threading.Tasks
open FSharp.Control.Reactive


type Profile = {
    Name : string
    AvatarUrl : string
    PopularRepositories : Repository seq
} and Repository = {
    Name : string
    Stars: int
    Languages: string[]
}

let reposResponseToPopularRespos = function
    | Ok(r) -> r |> parseUserRepos |> popularRepos
    | _ -> [||]
    
    
let languageResponseToRepoWithLanguages (repo : GitHubUserRepos.Root) (response: HttpResponse) = 
    match response with
    |Ok(l) -> {Name = repo.Name; Languages = (parseLanguages l); Stars = repo.StargazersCount}
    |_ -> {Name = repo.Name; Languages = Array.empty; Stars = repo.StargazersCount}

let toProfile = function
    | Ok(u), repos ->
        let user = parseUser u
        {Name = user.Name; PopularRepositories = repos; AvatarUrl = user.AvatarUrl} |> Some
    | _ -> None

let getProfile userName =
    
        let userStream = 
            userName 
            |> userUrl 
            |> asyncResponseToObservable


        let toRepoWithLanguagesStream (repo:GitHubUserRepos.Root) = 
            userName
            |> languagesUrl repo.Name
            |> asyncResponseToObservable
            |> Observable.map (languageResponseToRepoWithLanguages repo)

        let popularReposStream = 
            userName
            |> reposUrl
            |> asyncResponseToObservable
            |> Observable.map reposResponseToPopularRespos
            |> flatmap2 toRepoWithLanguagesStream
        
        async {
            return! popularReposStream
                    |> Observable.zip userStream
                    |> Observable.map toProfile
                    |> TaskObservableExtensions.ToTask
                    |> Async.AwaitTask
        }