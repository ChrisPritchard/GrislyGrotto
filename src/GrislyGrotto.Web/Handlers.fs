module Handlers
open Data
open Giraffe.ResponseWriters

let latest (data: GrislyData) = 
    query {
        for post in data.FullPosts () do
            sortByDescending post.Date
            take 5
            select post
    } |> Seq.toList |> List.map Views.listPost |> Views.layout [] |> htmlView