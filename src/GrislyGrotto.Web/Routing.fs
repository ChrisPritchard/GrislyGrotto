module Routing

open Giraffe

let webApp () =
    GET >=>
        choose [
            route "/site.css" >=> Content.siteCss ()
            route "/" >=> Handlers.latest ]