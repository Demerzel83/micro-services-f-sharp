module GitHub

open FSharp.Data

type GitHubUser = JsonProvider<"user.json">
type GitHubUserRepos = JsonProvider<"repos.json">

let parseUser = GitHubUser.Parse
let parseUserRepos = GitHubUserRepos.Parse

let host = "https://api.github.com"
let userUrl = sprintf "%s/users/%s" host
let reposUrl = sprintf "%s/users/%s/repos" host
let languagesUrl repoName userName = sprintf "%s/repos/%s/%s/languages" host userName repoName

let parseLanguages languagesJson =
    languagesJson
    |> JsonValue.Parse
    |> JsonExtensions.Properties
    |> Array.map fst

let popularRepos (repos: GitHubUserRepos.Root[]) =
    let ownRepos = repos |> Array.filter(fun repo -> not repo.Fork)
    let takeCount = if ownRepos.Length > 3 then 3 else repos.Length

    ownRepos
    |> Array.sortBy(fun r -> -r.StargazersCount)
    |> Array.toSeq
    |> Seq.take takeCount
    |> Seq.toArray

