module Views

open Giraffe.GiraffeViewEngine

let layout menuItems content =
    html [] [
        head [] [
            title []  [ encodedText "The Grisly Grotto" ]
            link [ _rel  "stylesheet"
                   _type "text/css"
                   _href "/site.css" ]
        ]
        body [] [
            h1 [] [ rawText "The Grisly Grotto" ]
            nav [] menuItems
            section [] content
        ]
    ]

let post (model: Models.Post) = 
    div [] [
        h2 [] [ encodedText model.title ]
        div [] [ rawText model.content ]
    ]