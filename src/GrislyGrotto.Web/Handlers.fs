module Handlers

open Giraffe
open Microsoft.AspNetCore.Http

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
            let postList = posts |> Seq.toList |> List.map Views.listPost |> Views.layout
            return! htmlView postList next ctx
        }