module Handlers

open Giraffe
open Microsoft.AspNetCore.Http
open Data
open System.Security.Claims
open Microsoft.AspNetCore.Authentication
open Microsoft.AspNetCore.Authentication.Cookies
open FSharp.Control.Tasks.ContextInsensitive 

let accessDenied : HttpHandler = 
    setStatusCode 401 >=> text "Access Denied"
let pageNotFound : HttpHandler = 
    setStatusCode 404 >=> text "Page Not Found"

let latest page = 
    fun (next : HttpFunc) (ctx : HttpContext) -> 
        task {
            let data = ctx.GetService<GrislyData> ()
            let skipCount = page * 5
            let posts = query {
                for post in data.FullPosts () do
                    sortByDescending post.Date
                    skip skipCount
                    take 5
                    select post
            }
            return! htmlView (Views.latest posts page) next ctx
        }

let single key = 
    fun (next : HttpFunc) (ctx : HttpContext) -> 
        task {
            let data = ctx.GetService<GrislyData> ()
            let post = query {
                for post in data.FullPosts () do
                    where (post.Key = key)
                    select post
                }
            return! 
                match Seq.tryHead post with
                | Some p -> htmlView (Views.single p) next ctx
                | None -> pageNotFound next ctx
        }

let login : HttpHandler = Views.login false |> htmlView

let private monthNames = 
    [""; "january"; "february"; "march"; "april";
    "may"; "june"; "july"; "august"; "september";
    "october"; "november"; "december"]

let archives = 
    fun (next : HttpFunc) (ctx : HttpContext) -> 
        task {
            let data = ctx.GetService<GrislyData> ()
            let allByDate = query {
                for post in data.FullPosts () do
                    sortBy post.Date
                    select (post.Date.Month, post.Date.Year)
            }   
            let years = 
                allByDate 
                |> Seq.groupBy (fun (_,year) -> year)
                |> Seq.map (fun (year,posts) -> 
                    year, posts 
                        |> Seq.groupBy (fun (month,_) -> month) 
                        |> Seq.map (fun (month,posts) -> monthNames.[month],Seq.length posts))
            let stories = query {
                for post in data.FullPosts () do
                    sortByDescending post.Date
                    where post.IsStory
                    select {
                        Key = post.Key
                        Title = post.Title
                        Author = post.Author
                        Date = post.Date
                        Content = ""
                        IsStory = true
                        WordCount = post.WordCount
                        Comments = new System.Collections.Generic.List<Comment>()
                    }
            }
            return! htmlView (Views.archives years stories) next ctx
        }

let private trimToSearchTerm (term:string) content =
    let stripped = System.Text.RegularExpressions.Regex.Replace(content, "<[^>]*>", "")
    let index = stripped.IndexOf(term)
    match index with 
    | -1 -> ""
    | _ -> 
        let start,stop = max (index - 20) 0, min (index + term.Length + 20) stripped.Length
        let section = stripped.Substring(start, stop - start)
        "..." + section + "..."


let search = 
    fun (next : HttpFunc) (ctx : HttpContext) -> 
        task {
            return! 
                match ctx.TryGetQueryStringValue "searchTerm" with
                | None -> htmlView (Views.search None) next ctx
                | Some term ->
                    let data = ctx.GetService<GrislyData> ()
                    let posts = query {
                            for post in data.FullPosts () do
                                where (post.Title.Contains(term) || post.Content.Contains(term))
                                sortByDescending post.Date
                                take 50
                                select post
                        } 
                    let results = 
                        posts 
                            |> Seq.map (fun p -> { p with Content = trimToSearchTerm term p.Content })
                            |> Seq.toList
                    htmlView (results |> Some |> Views.search) next ctx
        }

let editor key = 
    fun (next : HttpFunc) (ctx : HttpContext) -> 
        task {
            return! text "TBC" next ctx
        }

[<CLIMutable>]
type LoginForm = {
    username: string
    password: string
}

let setUserAndRedirect (next : HttpFunc) (ctx : HttpContext) (author: Author) =
    task {
        let issuer = "http://grislygrotto.azurewebsites.net/"
        let claims =
            [
                Claim(ClaimTypes.Name, author.Username,  ClaimValueTypes.String, issuer)
            ]
        let authScheme = CookieAuthenticationDefaults.AuthenticationScheme
        let identity = ClaimsIdentity(claims, authScheme)
        let user = ClaimsPrincipal(identity)
        do! ctx.SignInAsync(authScheme, user)
        
        return! redirectTo false "/" next ctx
    }

let tryLogin = 
    fun (next : HttpFunc) (ctx : HttpContext) -> 
        task {
            let! form = ctx.TryBindFormAsync<LoginForm> ()
            let badLogin () = htmlView (Views.login true) next ctx
            return! 
                match form with
                | Error _ -> badLogin ()
                | Ok form -> 
                    let data = ctx.GetService<GrislyData> ()
                    let authors = query {
                        for user in data.Authors do
                            where (user.Username = form.username)
                            select user
                    }
                    match Seq.tryHead authors with
                    | None -> badLogin ()
                    | Some a ->
                        if a.Validate form.password then
                            setUserAndRedirect next ctx a
                        else badLogin ()
        }

let createComment key = 
    fun (next : HttpFunc) (ctx : HttpContext) -> 
        task {
            return! text "TBC" next ctx
        }

let createPost = 
    fun (next : HttpFunc) (ctx : HttpContext) -> 
        task {
            return! text "TBC" next ctx
        }

let editPost key = 
    fun (next : HttpFunc) (ctx : HttpContext) -> 
        task {
            return! text "TBC" next ctx
        }