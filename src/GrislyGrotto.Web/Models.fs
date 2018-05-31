module Models
open System
open Microsoft.EntityFrameworkCore
open System.ComponentModel.DataAnnotations
open System.ComponentModel.DataAnnotations.Schema

[<CLIMutable>]
type Post = {
    [<Key>]
    Key: string
    Title: string
    [<ForeignKey("Author_Username")>]
    Author: Author
    Date: DateTime
    Content: string
    IsStory: bool
    WordCount: int
    Comments: Comment list
} and [<CLIMutable>]Author = {
    [<Key>]
    Username: string
    Password: string
    DisplayName: string
    ImageUrl: string
} and [<CLIMutable>]Comment = {
    Id: int
    [<ForeignKey("Post_Key")>]
    Post_Key: string
    Author: string
    Date: DateTime
    Content: string
}

type GrislyData (options) = 
    inherit DbContext (options)

    [<DefaultValue>] val mutable authors : DbSet<Author>
    member __.Authors with get() = __.authors and set v = __.authors <- v
    [<DefaultValue>] val mutable posts : DbSet<Post>
    member __.Posts with get() = __.posts and set v = __.posts <- v
    [<DefaultValue>] val mutable comments : DbSet<Comment>
    member __.Comments with get() = __.comments and set v = __.comments <- v