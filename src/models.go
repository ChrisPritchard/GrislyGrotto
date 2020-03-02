package main

type latestViewModel struct {
	Posts []blogPost
}

type blogPost struct {
	Author, Key, Title, Content string
	Date                        string
	IsStory                     bool
	WordCount, CommentCount     int
	Comments                    []comment
}

type author struct {
	Username, Password, DisplayName, ImageURL string
}

type comment struct {
	ID                    int
	Author, Content, Date string
}
