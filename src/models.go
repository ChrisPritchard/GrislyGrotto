package main

import "strings"

type latestViewModel struct {
	NotFirstPage bool
	PrevPage     int
	NextPage     int
	Posts        []blogPost
}

type singleViewModel struct {
	Post                blogPost
	OwnBlog, CanComment bool
	CommentError        string
}

type archivesViewModel struct {
	Years   []yearSet
	Stories []blogPost
}

type monthViewModel struct {
	Month, Year         string
	PrevMonth, PrevYear string
	NextMonth, NextYear string
	Posts               []blogPost
}

type searchViewModel struct {
	SearchTerm  string
	ZeroResults bool
	Results     []blogPost
}

type loginViewModel struct {
	Error string
}

type editorViewModel struct {
	NewPost          bool
	Title, Content   string
	IsStory, IsDraft bool
	PostError        string
}

type blogPost struct {
	Author, AuthorUsername  string
	Key, Title, Content     string
	Date                    string
	IsStory                 bool
	WordCount, CommentCount int
	Comments                []blogComment
}

type blogComment struct {
	ID                    int
	Author, Content, Date string
	Owned                 bool
}

func (post blogPost) isDraft() bool {
	return strings.HasPrefix(post.Title, draftPrefix)
}

type author struct {
	Username, Password, DisplayName, ImageURL string
}

type comment struct {
	ID                    int
	Author, Content, Date string
}

type yearSet struct {
	Year   string
	Months []monthCount
}

type monthCount struct {
	Month, Year string
	Count       int
}
