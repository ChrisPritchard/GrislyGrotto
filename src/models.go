package main

import "time"

type latestViewModel struct {
	Posts []blogPost
}

type blogPost struct {
	Author                  []author
	Key, Title, Content     string
	Date                    time.Time
	IsStory                 bool
	WordCount, CommentCount int
	Comments                []comment
}

type author struct {
	Username, Password, DisplayName, ImageURL string
}

type comment struct {
	ID              int
	Author, Content string
	Date            time.Time
}
