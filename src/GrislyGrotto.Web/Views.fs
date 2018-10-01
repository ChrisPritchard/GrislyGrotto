module Views

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
            footer [] [ rawText "Grisly Grotto v15.1. Site designed and coded by Christopher Pritchard, 2018" ]
            script [
                _src "https://chrispritchard.github.io/Wandering-Triangles/background-animation.js"
            ] []
            script [] [
                rawText 
                        "let anim = new GrislyGrotto.BackgroundAnimation()
                        anim.entityCount = 5
                        anim.initialise(
                                document.getElementById('backgroundCanvas'), 
                                'white', 
                                '#EEEEEE', 
                                '#CCCCCC')"
            ]
        ]
    ]

let buttonLink text cssClass url =
    form [ _method "GET"; _action url ] [ input [ _type "submit"; _value text; _class cssClass ] ]

let timeZone = TimeZoneInfo.FindSystemTimeZoneById("New Zealand Standard Time")
let formatDate (date : DateTime) = 
    let date = TimeZoneInfo.ConvertTimeFromUtc(date, timeZone)
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
                    li [] [
                        b [] [ sprintf "Commented by %s at %s:" c.Author date |> rawText ]
                        br []
                        rawText c.Content
                    ]) |> Seq.toList |> ul [ _class "comments" ]
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

let about isAuthor = 
    let content = [ 
        article [] [
            h2 [ _class "page-heading" ] [ rawText "About me" ]
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
                    However, it has been entirely coded using my new favourite language of choice, F#, in contrast to my 'day job' standard of C#.
                    F# is a functional, ML-syntax language that I find a pleasure to work with - its almost like C# with all the fat burnt off.
                    I started learning F# a few months prior to starting GG15, and haven't looked back :)"
            ]
            p [] [
                rawText "To work effectively in F# for web dev, I am using the web framework "
                a [ _href "https://github.com/giraffe-fsharp/Giraffe" ] [ rawText "Giraffe" ]
                rawText ", which is an ASP.NET Core extension that allows building an ASP.NET Core website using functional semantics."
                br []
                rawText "Giraffe includes the GiraffeViewEngine as an optional way to design HTML views entirely in code, which I have used. As a result, the shipped content for this site is just a dll and a few css/js files."
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