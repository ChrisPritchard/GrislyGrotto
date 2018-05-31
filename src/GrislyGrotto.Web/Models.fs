module Models
open System

type Post = {
    title: string
    author: string
    date: DateTime
    content: string
    isStory: bool
    comments: Comment list
} and Comment = {
    author: string
    date: DateTime
    content: string
}