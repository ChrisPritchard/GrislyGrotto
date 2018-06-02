module Handlers

open Giraffe
open Microsoft.AspNetCore.Http

let accessDenied : HttpHandler = 
    setStatusCode 401 >=> text "Access Denied"
let pageNotFound : HttpHandler = 
    setStatusCode 404 >=> text "Page Not Found"

let latest page = 
    fun (next : HttpFunc) (ctx : HttpContext) -> 
        task {
            let data = ctx.GetService<Data.GrislyData> ()
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
            let data = ctx.GetService<Data.GrislyData> ()
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

let archives = 
    fun (next : HttpFunc) (ctx : HttpContext) -> 
        task {
            return! text "TBC" next ctx
        }

let search = 
    fun (next : HttpFunc) (ctx : HttpContext) -> 
        task {
            return! text "TBC" next ctx
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

let runSearch = 
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