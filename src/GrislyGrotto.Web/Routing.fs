module Routing

open Giraffe

let webApp () =
    GET >=>
        choose [
            route "/site.css" >=> Content.css
            route "/" >=> Handlers.latest 0
            routef "/post/%s" Handlers.single ]