module Views

open Giraffe.GiraffeViewEngine

let layout isAuthor content =
    let menuItems = [
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
        head [] [
            title [] [ rawText "The Grisly Grotto" ]
            meta [ _name "description"
                   _content "The personal blog of Chris Pritchard and Peter Coleman" ]
            link [ _rel "shortcut icon" 
                   _type "image/x-icon"
                   _href "/favicon.png" ]
            link [ _rel "stylesheet"
                   _type "text/css"
                   _href "/site.css" ]
        ]
        body [] [
            header [] [ h1 [] [ a [ _href "/" ] [ rawText "The Grisly Grotto" ] ] ]
            nav [] menuItems
            section [] content
            footer [] [ rawText "Grisly Grotto v15. Site designed and coded by Christopher Pritchard, 2018" ]
        ]
    ]

let private listPost (post : Data.Post) = 
    article [] [
        header [] [ h2 [] [ a [ sprintf "/post/%s" post.Key |> _href ] [ encodedText post.Title ] ] ]
        footer [] [ 
                sprintf "posted by %s on %O" post.Author.DisplayName post.Date |> rawText
                a [ sprintf "/post/%s#comments" post.Key |> _href ] [ Seq.length post.Comments |> sprintf "comments (%i)" |> rawText ]
            ]
        section [] [ rawText post.Content ]
    ]

let latest isAuthor posts page = 
    let postList = posts |> Seq.toList |> List.map listPost
    let navLink pg txt = a [ sprintf "/page/%i" pg |> _href ] [ rawText txt ]
    let content = 
        postList @ 
        match page with 
        | 0 -> [ navLink 1 "Next Page" ] 
        | _ -> [ navLink (page - 1) "Previous Page";navLink (page + 1) "Next Page" ]
    layout isAuthor content

let private comment (c : Data.Comment) = [
        b [] [ sprintf "%s. %O" c.Author c.Date |> rawText ]
        p [] [ rawText c.Content ] 
    ]

type CommentsError = | NoCommentError | RequiredCommentFields | InvalidCommentContent

let single isAuthor isOwnedPost (post : Data.Post) commentError = 
    let content = 
        [ 
            article [] [
                header [] [ h1 [] [ encodedText post.Title ] ]
                footer [] [ sprintf "%s. %O" post.Author.DisplayName post.Date |> rawText ]
                section [] [ rawText post.Content ]
            ] 
        ]
        @
        if isOwnedPost then [ a [ sprintf "/edit/%s" post.Key |> _href ] [ rawText "Edit" ] ] else []
        @
        [ 
            div [] [
                a [ _name "comments" ] []
                h2 [] [ rawText "Comments" ]
                post.Comments |> Seq.collect comment |> Seq.toList |> div []
            ] 
        ]
    let commentForm = [
        form [ _method "POST" ] [
            div [] [
                label [] [
                    rawText "Author"
                    br []
                    input [ _type "text"; _name "author" ]
                ]
            ]
            div [] [
                label [] [
                    rawText "Content"
                    br []
                    textarea [ _rows "3"; _cols "50"; _name "content" ] []
                ]
            ]
            input [ _type "submit"; _value "Comment" ]
            (match commentError with
            | NoCommentError -> []
            | RequiredCommentFields -> [ rawText "Both author and content are required fields" ]
            | InvalidCommentContent -> [ rawText "Comments cannot contain links" ]) |> span []
        ]
    ]
    if post.Comments.Count >= 20 then 
        layout isAuthor content
    else
        layout isAuthor (content @ commentForm)

let login isAuthor wasError = 
    layout isAuthor [
        form [ _method "POST" ] [
            div [] [
                label [] [
                    rawText "Username"
                    br []
                    input [ _type "text"; _name "username" ]
                ]
            ]
            span [] [ rawText (if wasError then "Username and/or Password not recognised" else "") ]
            div [] [
                label [] [
                    rawText "Password"
                    br []
                    input [ _type "password"; _name "password" ]
                ]
            ]
            input [ _type "submit"; _value "Login" ]
        ]
    ]

let archives isAuthor (years : seq<int * seq<string * int>>) (stories : seq<Data.Post> ) = 
    let yearList = 
        years |> Seq.map (fun (y,months) -> 
            div [] [ 
                h3 [] [ string y |> rawText ]
                months |> Seq.map (fun (m,c) -> 
                    li [] [ 
                        a [ sprintf "/month/%s/%i" m y |> _href ] [ sprintf "%s (%i)" m c |> rawText ] 
                    ]) |> Seq.toList |> ul []
             ]) |> Seq.toList
    let storyList = 
        stories |> Seq.map (fun p -> 
            div [] [
                h3 [] [ a [ sprintf "/post/%s" p.Key |> _href ] [ sprintf "%s (%i words)" p.Title p.WordCount |> rawText ] ]
                span [] [ sprintf "Posted by %s on %O" p.Author.DisplayName p.Date |> rawText ]
            ]) |> Seq.toList
    let content = [
            [h2 [] [ rawText "Archives" ]]
            yearList
            [h2 [] [ rawText "Stories" ]]
            storyList
        ]
    List.concat content |> layout isAuthor

let month isAuthor posts prevUrl nextUrl = 
    let postList = posts |> Seq.toList |> List.map listPost
    let navLink url txt = a [ _href url ] [ rawText txt ]
    let content = 
        postList
        @ 
        [ navLink prevUrl "Previous Month"; navLink nextUrl "Next Month" ]
    layout isAuthor content

let search isAuthor (results: Data.Post list option) =
    let searchBox = [
            h2 [] [ rawText "Search" ]
            form [ _method "GET" ] [
                p [] [
                    label [] [ 
                        rawText "Search term"
                        br []
                        input [ _type "text"; _name "searchTerm" ] ]
                    ]
                input [ _type "submit"; _value "Search" ]
                p [] [ rawText "Max 50 results. Note, searches can take some time." ]
            ]
        ]
    match results with
    | None -> layout isAuthor searchBox
    | Some r -> 
        let results = [
            h3 [] [ rawText "Results" ]
            r |> List.map (fun post -> 
                li [] [
                    h4 [] [ rawText post.Title ]
                    p [] [ rawText post.Content ]
                    span [] [ sprintf "Posted by %s on %O" post.Author.DisplayName post.Date |> rawText ]
                ]) |> ul []
        ]
        layout isAuthor (searchBox @ results)


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

let editor (post : PostViewModel) autosave errors = 
    layout true [
        form [ _method "POST" ] [
            p [] [
                label [] [
                    rawText "Title"
                    br []
                    input [ _type "text"; _name "title"; _value post.title ]
                ]
                (match errors with
                | NoEditorErrors -> []
                | RequiredEditorFields -> [ rawText "Both title and content are required fields" ]
                | ExistingPostKey -> [ rawText "A post with a similar title already exists" ]) |> span []
            ]
            p [] [
                label [] [
                    rawText "Content"
                    br []
                    input [ _type "hidden"; _name "content"; _id "content" ]
                    div [ _class "editor"; _contenteditable "true"; _id "editor" ] [ rawText post.content ]
                ]
            ]
            p [] [
                label [] [
                    input [ _name "editmode"; _type "radio"; _value "rendered"; _checked ]
                    rawText "Rendered"
                ]
                label [] [
                    input [ _name "editmode"; _type "radio"; _value "html" ]
                    rawText "HTML"
                ]
            ]
            p [] [
                input [ _type "hidden"; _name "isStory"; _id "isStory"; _value "false" ]
                label [] [
                    [ _id "isStoryToggle"; _type "checkbox" ] @ (if post.isStory then [ _checked ] else []) |> input
                    rawText "Is Story"
                ]
            ]
            input [ _type "submit"; _value "Submit"; _id "submit" ]
            (match autosave with | AutoSaveEnabled -> span [ _id "saving-status" ] [] | _ -> br [])
            script [ _type "text/javascript"; _src "/editor.js" ] []
        ]
    ]

let about isAuthor = 
    let content = [ 
        article [] [
            h1 [] [ rawText "About me" ]
            p [] [
                rawText 
                    "My name is Christopher Pritchard, and I work as a senior-level software developer and architect in Wellington, New Zealand. 
                    I operate as a contractor, preferring short to mid-term length contracts (1-8 months), so I can work on lots of different things with different people, while also being able to set time aside for my own projects.
                    In the past, I have worked for long periods as a full-time employee (largely at the IT vendor Provoke Solutions), generally in the Microsoft technology stack."
            ]
            p [] [
                rawText 
                    "I have been working in the IT industry for over twelve years now, primary in development or development-related roles. While management, 
                    solution design and technical leadership have increasingly been part of my responsibilities, I always prefer to stay close to the code. 
                    I think I'm pretty good at what I do :D"
            ]
            p [] [
                rawText 
                    "Outside of work, my hobbies are writing (successive participant/winner in the NaNoWriMo competition each year I'm proud to say), and gaming. I also do a lot of personal projects, such as the one you are looking at now.
                    Finally, and by no means least, I am a happily married man with a young daughter, as well as a small dog."
            ]
            h2 [] [ rawText "Contact" ]
            p [] [
                rawText "I can be reached most easily by my personal email address: "
                a [ _href "mailto:chrispritchard.nz@gmail.com" ] [ rawText "chrispritchard.nz@gmail.com." ]
                br []
                rawText "My LinkedIn profile is "
                a [ _href "https://nz.linkedin.com/pub/christopher-pritchard/a/9b6/a66" ] [ rawText "here." ]
                br []
                rawText "My Github profile is "
                a [ _href "https://github.com/ChrisPritchard" ] [ rawText "here." ]
            ]
            h2 [] [ rawText "About the Grisly Grotto" ]
            p [] [
                rawText 
                    "The first version of GG went live back in 2006. I had always wanted to have a blog, and thought having one was part of what made someone a developer.
                    More importantly though, while I had learned much of the craft of web development (in ASP.NET Webforms back then), my day job did not afford me the opportunity to 'do it all': design, development, hosting, bug fixing etc. 
                    So GG was born, a web application of manageable scope that allowed me to fully practice my craft. 
                    While my skill set is now vastly greater than the meagre needs of this site, it still serves as a periodic 'zen' meditation: rebuilding the same structure over and over again, in the latest JS framework, database architecture or server-side technology that takes my fancy."
            ]
            p [] [
                rawText 
                    "So far there have been 15 versions, each one distinctive. Earlier versions used ASP.NET Webforms, some used XSLT transforms for markup, some were pure client side javascript (including a one-off version in pure NodeJS). 
                    Multiple design frameworks have been used, although mostly I have done the CSS entirely myself for practice. At least six different data storage solutions (databases or otherwise) have been used, and multiple different hosting solutions before it ended up here, on Azure for v11+. 
                    I expect there to be many more iterations as I seek to learn new things going forward."
            ]
            h2 [] [ rawText "About v15 specifically" ]
            p [] [
                rawText 
                    "This version still uses ASP.NET Core like the previous version, and has been built using VS Code.
                    However, it marks a major change as well, as it has been entirely built using my new favourite language of choice, F#, in contrast to my 'day job' standard of C#.
                    F# is a functional, ML syntax language that I find a pleasure to work with, almost like C# with the fat burnt off and with the best coding style baked in as the default.
                    To work effectively in F# for web dev, I am using the web framework "
                a [ _href "https://github.com/giraffe-fsharp/Giraffe" ] [ rawText "Giraffe" ]
                rawText ", which is a ASP.NET extension based on the excellent standalone F# Suave framework."
                br []
                rawText "As a result of the use of Giraffe program, the HTML views are expressed with the in-code Graffe View Engine. Aside from code, there are no seperate files at all!"
            ]
            p [] [
                rawText 
                    "Other than the above, v15 shares a lot with v14: Database storage is Azure SQL Server, and web hosting is as an Azure App Service. I host the source code in Visual Studio Team Services, in a Git repository. 
                    VSTS provides a build &amp; release pipeline which automatically deploys the latest code into Azure."
            ]
            p [] [ rawText "Cheers." ]
        ] 
    ]
    layout isAuthor content