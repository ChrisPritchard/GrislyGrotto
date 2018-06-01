module Views

open Giraffe.GiraffeViewEngine

let layout content =
    let menuItems = [
        ul [] [
            li [] [ a [ _href "/login" ] [ rawText "Login" ] ]
            li [] [ a [ _href "/archives" ] [ rawText "Archives" ] ]
            li [] [ a [ _href "/search" ] [ rawText "Search" ] ]
            li [] [ a [ _href "/about" ] [ rawText "About" ] ]
        ]
    ]
    html [] [
        head [] [
            title []  [ encodedText "The Grisly Grotto" ]
            link [ _rel  "stylesheet"
                   _type "text/css"
                   _href "/site.css" ]
        ]
        body [] [
            header [] [ h1 [] [ rawText "The Grisly Grotto" ] ]
            nav [] menuItems
            section [] content
            footer [] [ rawText "Grisly Grotto v15. Site designed and coded by Christopher Pritchard, 2018" ]
        ]
    ]

let listPost (model: Data.Post) = 
    let link = sprintf "/post/%s" model.Key |> _href
    article [] [
        header [] [ h2 [] [ a [ link ] [ encodedText model.Title ] ] ]
        footer [] [ 
                sprintf "posted by %s on %O" model.Author.DisplayName model.Date |> rawText
                a [ link ] [ Seq.length model.Comments |> sprintf "comments (%i)" |> rawText ]
            ]
        section [] [ rawText model.Content ]
    ]