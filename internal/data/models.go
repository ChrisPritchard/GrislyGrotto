package data

import (
	"strings"

	"github.com/ChrisPritchard/GrislyGrotto/internal/config"
)

type BlogPost struct {
	Author, AuthorUsername  string
	Key, Title, Content     string
	Date                    string
	IsStory                 bool
	WordCount, CommentCount int
	Comments                []BlogComment
}

type BlogComment struct {
	ID                    int
	Author, Content, Date string
	Owned                 bool
}

func (post BlogPost) IsDraft() bool {
	return strings.HasPrefix(post.Title, config.DraftPrefix)
}

type Author struct {
	Username, Password, DisplayName, ImageURL string
}

type YearSet struct {
	Year   string
	Months []MonthCount
}

type MonthCount struct {
	Month, Year string
	Count       int
}
