package main

import "time"

// LatestViewModel holds data for rendering the view Latest
type LatestViewModel struct {
	Posts []BlogPost
}

// BlogPost represents a single blog post by an author
type BlogPost struct {
	Title, Body string
	Date        time.Time
}
