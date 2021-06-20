package handlers

import (
	"github.com/ChrisPritchard/GrislyGrotto/internal/data"
)

type latestViewModel struct {
	NotFirstPage bool
	PrevPage     int
	NextPage     int
	Posts        []data.BlogPost
}

type singleViewModel struct {
	Post                    data.BlogPost
	OwnBlog, CanComment     bool
	CSRFToken, CommentError string
}

type archivesViewModel struct {
	Years   []data.YearSet
	Stories []data.BlogPost
}

type monthViewModel struct {
	Month, Year         string
	PrevMonth, PrevYear string
	NextMonth, NextYear string
	Posts               []data.BlogPost
}

type searchViewModel struct {
	SearchTerm  string
	ZeroResults bool
	Results     []data.BlogPost
}

type loginViewModel struct {
	CSRFToken, Error string
}

type accountDetailsViewModel struct {
	Username                      string
	DisplayName, DisplayNameError string
	DisplayNameSuccess            bool
	ImageError                    string
	ImageSuccess                  bool
	PasswordError                 string
	PasswordSuccess               bool
	CSRFToken                     string
}

type editorViewModel struct {
	NewPost              bool
	Title, Content       string
	IsStory, IsDraft     bool
	CSRFToken, PostError string
}
