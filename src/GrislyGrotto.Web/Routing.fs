module Routing
open Giraffe
open Microsoft.AspNetCore.Http

let webApp () =
    GET >=>
        choose [
            route "/site.css" >=> Content.siteCss ()
            route "/" >=> warbler (fun (_, (ctx:HttpContext)) -> Handlers.latest (ctx.GetService<Data.GrislyData>()) ) ]