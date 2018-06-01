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

let listPost (model: Models.Post) = 
    div [] [
        h2 [] [ encodedText model.Title ]
        span [] [ 
                sprintf "posted by %s on %O" model.Author.DisplayName model.Date |> rawText
                a [ _href "#" ] [ Seq.length model.Comments |> sprintf "comments (%i)" |> rawText ]
            ]
        div [] [ rawText model.Content ]
    ]