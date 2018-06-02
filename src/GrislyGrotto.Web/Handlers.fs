module Handlers

open Giraffe
open Microsoft.AspNetCore.Http
open Data

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

let login = 
    fun (next : HttpFunc) (ctx : HttpContext) -> 
        task {
            return! text "TBC" next ctx
        }

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

let search = 
    fun (next : HttpFunc) (ctx : HttpContext) -> 
        task {
            return! 
                match ctx.TryGetQueryStringValue "searchTerm" with
                | None -> htmlView (Views.search None) next ctx
                | Some term ->
                    let results = []
                    htmlView (results |> Some |> Views.search) next ctx
        }

let editor key = 
    fun (next : HttpFunc) (ctx : HttpContext) -> 
        task {
            return! text "TBC" next ctx
        }

let tryLogin = 
    fun (next : HttpFunc) (ctx : HttpContext) -> 
        task {
            return! text "TBC" next ctx
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