module GrislyGrotto.Views

open System.IO
open HeyRed.MarkdownSharp
open Giraffe.GiraffeViewEngine
open System

let layout isAuthor content =
    let head = 
        head [] [
            title [] [ rawText "The Grisly Grotto" ]
            meta [ _name "description"
                   _content "The personal blog of Chris Pritchard and Peter Coleman" ]
            meta [ _charset "UTF-8" ]
            link [ _rel "shortcut icon" 
                   _type "image/x-icon"
                   _href "/favicon.png" ]
            link [ _rel "stylesheet"
                   _type "text/css"
                   _href "/site.css?v=2" ]
        ]
    let sitehead = header [] [ h1 [] [ a [ _href "/" ] [ rawText "The Grisly Grotto" ] ] ]
    let navigation = 
        nav [] [
            ul [] [
                li [] [ a 
                    [ _href (if isAuthor then "/new" else "/login") ] 
                    [ rawText (if isAuthor then "New Post" else "Login") ] ]
                li [] [ a [ _href "/archives" ] [ rawText "Archives" ] ]
                li [] [ a [ _href "/search" ] [ rawText "Search" ] ]
                li [] [ a [ _href "/about" ] [ rawText "About" ] ]
            ]
        ]
    html [] [
        head
        body [] [
            canvas [ _id "backgroundCanvas"; _style "position: absolute; z-index:-1;" ] []
            sitehead
            navigation
            section [ _class "content" ] content
            footer [] [ rawText "Grisly Grotto v16.0. Site designed and coded by Christopher Pritchard, 2019" ]
        ]
    ]

let buttonLink text cssClass url =
    form [ _method "GET"; _action url ] [ input [ _type "submit"; _value text; _class cssClass ] ]

let formatDate (date : DateTime) = 
    date.ToString("hh:mm tt, 'on' dddd, dd MMMM yyyy")

let private listPost (post : Data.Post) = 
    let content = 
        if post.IsStory || post.WordCount > 2000 then 
            sprintf "<p><a href='/post/%s'>Click through to read (%i words)...</a></p>" post.Key post.WordCount 
        else post.Content
    let date = formatDate post.Date
    article [] [
        h2 [] [ a [ sprintf "/post/%s" post.Key |> _href ] [ encodedText post.Title ] ]
        h5 [] [ 
                rawText <| sprintf "Posted by %s at %s. " post.Author.DisplayName date
                a [ sprintf "/post/%s#comments" post.Key |> _href ] [ Seq.length post.Comments |> sprintf "Comments (%i)" |> rawText ]
            ]
        section [ _class "post-content" ] [ rawText content ]
    ]

let latest isAuthor posts page = 
    let postList = posts |> Seq.toList |> List.map listPost
    let content = 
        postList @ 
        match page with 
        | 0 -> [ buttonLink "Next Page" "next-btn" <| sprintf "/page/%i" 1 ] 
        | _ -> 
            [ 
                buttonLink "Previous Page" "prev-btn" <| sprintf "/page/%i" (page - 1)
                buttonLink "Next Page" "next-btn" <| sprintf "/page/%i" (page + 1) ]
    layout isAuthor content

type CommentsError = | NoCommentError | RequiredCommentFields | InvalidCommentContent

let single isAuthor isOwnedPost (post : Data.Post) commentError = 
    let date = formatDate post.Date
    let content = 
        [ 
            article [] [
                h2 [] [ encodedText post.Title ]
                h5 [] [ rawText <| sprintf "Posted by %s at %s." post.Author.DisplayName date ]
                section [ _class "post-content" ] [ rawText post.Content ]
            ]
        ]
        @
        if isOwnedPost then 
            [ 
                form [ _method "GET"; _class "owned-post-btn"; _action <| sprintf "/edit/%s" post.Key ] 
                    [ input [ _type "submit"; _value "Edit"; _class "owned-post-btn" ] ]
                form [ _method "POST"; _class "owned-post-btn"; _action <| sprintf "/delete/%s" post.Key ] 
                    [ input [ _type "submit"; _value "Delete"; _class "owned-post-btn"; attr "onclick" "return confirm('Are you sure? This is irreversible.');" ] ]
                ] 
            else []
        @
        [ 
            div [] [
                a [ _name "comments" ] []
                h2 [] [ rawText "Comments" ]
                post.Comments |> Seq.map (fun c ->
                    let date = formatDate c.Date
                    let elements = [
                        b [] [ sprintf "Commented by %s at %s:" c.Author date |> rawText ]
                        br []
                        encodedText c.Content
                    ]
                    let deleteButton = 
                        form [ _method "POST"; _class "delete-comment-btn"; _action <| sprintf "/delete-comment/%i" c.Id ] 
                            [ input [ _type "submit"; _value "Delete"; _class "delete-comment-btn"; attr "onclick" "return confirm('Are you sure? This is irreversible.');" ] ]
                    li [] (if isOwnedPost then elements @ [deleteButton] else elements)) 
                    |> Seq.toList |> ul [ _class "comments" ]
            ] 
        ]
    let commentForm = [
        form [ _method "POST" ] [
            fieldset [] [
                label [ _for "author" ] [ rawText "Author" ]
                input [ _type "text"; _id "author"; _name "author" ]

                label [ _for "content" ] [ rawText "Content" ]
                textarea [ _rows "3"; _cols "50"; _id "content"; _name "content" ] []

                input [ _type "submit"; _value "Comment" ]

                (match commentError with
                | NoCommentError -> []
                | RequiredCommentFields -> [ rawText "Both author and content are required fields" ]
                | InvalidCommentContent -> [ rawText "Comments cannot contain links" ]) 
                    |> span [ _class "error-message" ]
            ]
        ]
    ]
    if post.Comments.Count >= 20 then 
        layout isAuthor content
    else
        layout isAuthor (content @ commentForm)

let login isAuthor wasError = 
    layout isAuthor [
        h2 [ _class "page-heading" ] [ rawText "Login as an Author" ]
        form [ _method "POST"; _class "login-box" ] [
            fieldset [] [
                label [ _for "username" ] [ rawText "Username" ]
                input [ _type "text"; _id "username"; _name "username" ]

                label [ _for "password" ] [ rawText "Password" ]
                input [ _type "password"; _id "password"; _name "password" ]

                label [ _class "error-message" ] [ rawText (if wasError then "Username and/or Password not recognised" else "") ]
                input [ _type "submit"; _value "Login" ]
            ]
        ]
    ]

let archives isAuthor (years : seq<int * seq<string * int>>) (stories : seq<Data.Post> ) = 
    let yearList = 
        years |> Seq.map (fun (y,months) -> 
            li [] [ 
                h3 [] [ string y |> rawText ]
                months |> Seq.map (fun (m,c) -> 
                    li [] [ 
                        a [ sprintf "/month/%s/%i" m y |> _href ] [ sprintf "%s (%i)" m c |> rawText ] 
                    ]) |> Seq.toList |> ul [ _class "months" ]
             ]) |> Seq.toList |> ul [ _class "years" ]
    let storyList = 
        stories |> Seq.map (fun p -> 
            let date = formatDate p.Date
            li [] [
                h3 [] [ a [ sprintf "/post/%s" p.Key |> _href ] [ sprintf "%s (%i words)" p.Title p.WordCount |> rawText ] ]
                span [] [ sprintf "Posted by %s at %s" p.Author.DisplayName date |> rawText ]
            ]) |> Seq.toList |> ul [ _class "stories" ]
    let content = [
            h2 [ _class "page-heading" ] [ rawText "Archives by Year" ]
            yearList
            h2 [ _class "page-heading" ] [ rawText "Stories by Date" ]
            storyList
        ]
    layout isAuthor content

let month isAuthor (monthName : string) year posts prevUrl nextUrl = 
    let postList = posts |> Seq.toList |> List.map listPost
    let capitilisedMonth = (Seq.head monthName |> Char.ToUpper)::(Seq.tail monthName |> Seq.toList) |> String.Concat
    let content = 
        [ h2 [ _class "page-heading" ] [ rawText <| sprintf "%s, %i" capitilisedMonth year ] ]
        @
        postList
        @ 
        [ buttonLink "Previous Month" "prev-btn" prevUrl; buttonLink "Next Month" "next-btn" nextUrl ]
    layout isAuthor content

let search isAuthor searchTerm (results: Data.Post list option) =
    let searchBox = [
            h2 [ _class "page-heading" ] [ rawText "Search for term" ]
            form [ _method "GET" ] [
                fieldset [] [
                    input [ _type "text"; _name "searchTerm"; _value searchTerm ]
                    input [ 
                        _type "submit"
                        _value "Search"
                        _class "pure-button pure-button-primary"
                        attr "onclick" "this.style.display = 'none';return true;" ]
                    span [] [ rawText "Max 50 results. Note, searches can take some time." ]
                ]
            ]
        ]
    layout isAuthor
        [ div [ _class "search-page" ]
            <| match results with
                | None -> searchBox
                | Some r -> 
                    let results = [
                        h3 [] [ rawText <| sprintf "Results for '%s'" searchTerm ]
                        r |> List.map (fun post -> 
                            let date = formatDate post.Date
                            li [] [
                                a [ _href <| sprintf "/post/%s" post.Key ] [ h4 [] [ rawText post.Title ] ]
                                p [] [ rawText post.Content ]
                                span [] [ sprintf "Posted by %s at %s" post.Author.DisplayName date |> rawText ]
                            ]) |> ul []
                    ]
                    searchBox @ results
        ]


[<CLIMutable>]
type PostViewModel = {
    title: string
    content: string
    isStory: bool
}

type EditorErrors = 
    | NoEditorErrors | RequiredEditorFields | ExistingPostKey

type EditorAutoSave = 
    | AutoSaveEnabled | AutoSaveDisabled

let editor isNew (post : PostViewModel) autosave errors = 
    layout true [
        h2 [ _class "page-heading" ] [ rawText <| if isNew then "New Post" else "Edit Post" ]
        form [ _method "POST" ] [
            fieldset [] [
                label [ _for "title" ] [ rawText "Title" ]
                input [ _type "text"; _id "title"; _name "title"; _value post.title ]
                (match errors with
                | NoEditorErrors -> []
                | RequiredEditorFields -> [ rawText "Both title and content are required fields" ]
                | ExistingPostKey -> [ rawText "A post with a similar title already exists" ]) 
                    |> span [ _class "error-message" ]

                label [ _for "editor" ] [ rawText "Content" ]
                input [ _type "hidden"; _name "content"; _id "content" ]
                div [ _class "editor"; _contenteditable "true"; _id "editor" ] [ rawText post.content ]

                div [ _class "inline" ] [
                    label [] [
                        input [ _name "editmode"; _type "radio"; _value "rendered"; _checked ]
                        rawText "Rendered"
                    ]
                    label [] [
                        input [ _name "editmode"; _type "radio"; _value "html" ]
                        rawText "HTML"
                    ]
                    input [ _type "hidden"; _name "isStory"; _id "isStory"; _value (string post.isStory) ]
                    label [] [
                        [ _id "isStoryToggle"; _type "checkbox" ] @ (if post.isStory then [ _checked ] else []) |> input
                        rawText "Is Story"
                    ]
                ]
                
                input [ _type "submit"; _value "Submit"; _id "submit" ]
                (match autosave with | AutoSaveEnabled -> span [ _id "saving-status" ] [] | _ -> br [])

                script [ _type "text/javascript"; _src "/editor.js?v=2" ] []
            ]
        ]
    ]

let aboutContent = 
    let markdownText = File.ReadAllText "./about-content.md"
    let transformer = Markdown()
    transformer.Transform markdownText

let about isAuthor = 
    let content = [ 
        article [] [
            h2 [ _class "page-heading" ] [ rawText "About me" ]
            rawText aboutContent
        ] 
    ]
    layout isAuthor content
