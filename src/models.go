package main

import "time"

type latestViewModel struct {
	Posts []blogPost
}

type blogPost struct {
	Title, Body string
	Date        time.Time
}
