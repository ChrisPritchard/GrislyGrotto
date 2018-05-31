module Views

open Giraffe.GiraffeViewEngine

let layout (content: XmlNode list) =
    html [] [
        head [] [
            title []  [ encodedText "The Grisly Grotto" ]
            link [ _rel  "stylesheet"
                   _type "text/css"
                   _href "/site.css" ]
        ]
        body [] content
    ]

let partial () =
    h1 [] [ encodedText "The Grisly Grotto" ]

let index (model : string) =
    [
        partial()
        p [] [ encodedText model ]
    ] |> layout