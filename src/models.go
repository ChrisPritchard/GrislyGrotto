package main

type latestViewModel struct {
	NotFirstPage bool
	PrevPage     int
	NextPage     int
	Posts        []blogPost
}

type singleViewModel struct {
	CanComment bool
	Post       blogPost
}

type searchViewModel struct {
	SearchTerm  string
	ZeroResults bool
	Results     []blogPost
}

type archivesViewModel struct {
	Years   []yearSet
	Stories []blogPost
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

type yearSet struct {
	Year   string
	Months []monthCount
}

type monthCount struct {
	Month, Year string
	Count       int
}