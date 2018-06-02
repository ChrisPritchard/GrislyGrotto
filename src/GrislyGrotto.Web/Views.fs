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

let listPost (post : Data.Post) = 
    let link = sprintf "/post/%s" post.Key |> _href
    article [] [
        header [] [ h2 [] [ a [ link ] [ encodedText post.Title ] ] ]
        footer [] [ 
                sprintf "posted by %s on %O" post.Author.DisplayName post.Date |> rawText
                a [ link ] [ Seq.length post.Comments |> sprintf "comments (%i)" |> rawText ]
            ]
        section [] [ rawText post.Content ]
    ]

let latest posts page = 
    let postList = posts |> Seq.toList |> List.map listPost
    let navLink pg txt = a [ sprintf "/page/%i" pg |> _href ] [ rawText txt ]
    let content = 
        postList @ 
        match page with 
        | 0 -> [ navLink 1 "Next" ] 
        | _ -> [ navLink (page - 1) "Prev";navLink (page + 1) "Next" ]
    layout content

let single (post : Data.Post) = 
    let comment (c : Data.Comment) = 
        [
            b [] [ sprintf "%s. %O" c.Author c.Date |> rawText ]
            p [] [ rawText c.Content ] 
        ]
    let content = 
        [ 
            article [] [
                header [] [ h1 [] [ encodedText post.Title ] ]
                footer [] [ sprintf "%s. %O" post.Author.DisplayName post.Date |> rawText ]
                section [] [ rawText post.Content ]
            ]
            div [] [
                h2 [] [ rawText "Comments" ]
                post.Comments |> Seq.collect comment |> Seq.toList |> div []
            ]
        ]
    layout content

let pageNotFound =
    let content = [ h1 [] [ rawText "Page Not Found" ] ]
    layout content