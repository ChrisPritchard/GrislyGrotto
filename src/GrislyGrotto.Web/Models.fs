module Models
open System
open Microsoft.EntityFrameworkCore

type Post = {
    Key: string
    Title: string
    Author: Author
    Date: DateTime
    Content: string
    IsStory: bool
    WordCount: int
    Comments: Comment list
} and Author = {
    Username: string
    Password: string
    DisplayName: string
    ImageUrl: string
} and Comment = {
    author: string
    date: DateTime
    content: string
}

type GrislyData (options) = 
    inherit DbContext (options)

    member val Authors = Unchecked.defaultof<DbSet<Author>> with get,set
    member val Posts = Unchecked.defaultof<DbSet<Post>> with get,set
    member val Comments = Unchecked.defaultof<DbSet<Comment>> with get,set