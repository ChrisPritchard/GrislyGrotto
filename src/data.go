package main

import (
	"database/sql"
	"time"
)

const dbName = "./grislygrotto.db"

func getLatestPosts() ([]blogPost, error) {
	database, err := sql.Open("sqlite3", dbName)
	if err != nil {
		return nil, err
	}

	rows, err := database.Query("SELECT Title, Content, Date FROM Posts ORDER BY Date DESC LIMIT 5")

	posts := make([]blogPost, 0)
	var title, content string
	var date time.Time
	for rows.Next() {
		rows.Scan(&title, &content, &date)
		posts = append(posts, blogPost{title, content, date})
	}

	return posts, nil
}
