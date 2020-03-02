package main

import (
	"database/sql"
)

const dbName = "./grislygrotto.db"

func getLatestPosts() ([]blogPost, error) {
	database, err := sql.Open("sqlite3", dbName)
	if err != nil {
		return nil, err
	}

	rows, err := database.Query("SELECT Title, Content, Date FROM Posts ORDER BY Date DESC LIMIT 5")

	posts := make([]blogPost, 0)
	var post blogPost
	for rows.Next() {
		rows.Scan(&post.Title, &post.Content, &post.Date)
		posts = append(posts, post)
	}

	return posts, nil
}

func getSinglePost(key string) (post blogPost, err error) {
	database, err := sql.Open("sqlite3", dbName)
	if err != nil {
		return post, err
	}

	_, err = database.Query("SELECT Title, Content, Date FROM Posts WHERE Key = ?", key)

	// var title, content string
	// var date time.Time
	// for rows.Next() {
	// 	rows.Scan(&title, &content, &date)
	// 	posts = append(posts, blogPost{title, content, date})
	// }

	return post, nil
}
