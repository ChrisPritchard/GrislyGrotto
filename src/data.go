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

	rows, err := database.Query(`
		SELECT 
			(SELECT DisplayName FROM Authors WHERE Username = p.Author_Username) as Author,
			p.Key, p.Title, p.Content, p.Date, p.IsStory, p.WordCount, 
			(SELECT COUNT(*) FROM Comments WHERE Post_Key = p.Key) as CommentCount
		FROM Posts p
		ORDER BY p.Date DESC 
		LIMIT 5`)
	if err != nil {
		return nil, err
	}

	posts := make([]blogPost, 0)
	var post blogPost
	for rows.Next() {
		err = rows.Scan(
			&post.Author, &post.Key, &post.Title, &post.Content,
			&post.Date, &post.IsStory, &post.WordCount, &post.CommentCount)
		if err != nil {
			return nil, err
		}
		posts = append(posts, post)
	}

	return posts, nil
}

func getSinglePost(key string) (post blogPost, err error) {
	database, err := sql.Open("sqlite3", dbName)
	if err != nil {
		return post, err
	}

	row := database.QueryRow(`
		SELECT 
			(SELECT DisplayName FROM Authors WHERE Username = p.Author_Username) as Author,
			p.Title, p.Content, p.Date 
		FROM Posts p
		WHERE Key = ?`, key)
	err = row.Scan(&post.Author, &post.Title, &post.Content, &post.Date)

	if err != nil {
		return post, err
	}

	post.Comments, err = getPostComments(database, key)

	return post, err
}

func getPostComments(database *sql.DB, key string) (comments []comment, err error) {
	comments = make([]comment, 0)
	rows, err := database.Query(`
		SELECT 
			Id, Author, Date, Content
		FROM Comments
		WHERE Post_Key = ?
		ORDER BY Date`, key)

	if err != nil {
		return comments, nil
	}

	var comment comment
	for rows.Next() {
		err = rows.Scan(
			&comment.ID, &comment.Author, &comment.Date, &comment.Content)
		if err != nil {
			return comments, err
		}
		comments = append(comments, comment)
	}

	return comments, err
}
