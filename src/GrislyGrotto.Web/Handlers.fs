module Handlers

open Giraffe
open Microsoft.AspNetCore.Http

let latest = 
    fun (next : HttpFunc) (ctx : HttpContext) -> 
        task {
            let data = ctx.GetService<Data.GrislyData> ()
            let posts = query {
                for post in data.FullPosts () do
                    sortByDescending post.Date
                    take 5
                    select post
            }
            let html = posts |> Seq.toList |> List.map Views.listPost |> Views.layout []
            return! htmlView html next ctx
        }