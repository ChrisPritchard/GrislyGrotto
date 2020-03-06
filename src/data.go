package main

import (
	"crypto/sha512"
	"database/sql"
	"encoding/base64"
	"errors"
	"regexp"
	"strconv"
	"strings"
	"time"
)

func getLatestPosts(page int) ([]blogPost, error) {
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
		LIMIT ? OFFSET ?`, pageLength, page*pageLength)
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

func getSinglePost(key string) (post blogPost, pageNotFound bool, err error) {
	database, err := sql.Open("sqlite3", dbName)
	if err != nil {
		return post, false, err
	}

	row := database.QueryRow(`
		SELECT 
			(SELECT DisplayName FROM Authors WHERE Username = p.Author_Username) as Author,
			p.Title, p.Content, p.Date, p.Author_Username 
		FROM Posts p
		WHERE Key = ?`, key)
	err = row.Scan(&post.Author, &post.Title, &post.Content, &post.Date, &post.AuthorUsername)

	if err != nil {
		if err == sql.ErrNoRows {
			return post, true, nil
		}
		return post, false, err
	}

	post.Key = key
	post.Comments, err = getPostComments(database, key)

	return post, false, err
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

func addCommentToBlog(author, content, postKey string) (err error) {
	database, err := sql.Open("sqlite3", dbName)
	if err != nil {
		return err
	}

	date := time.Now().Format("2006-01-02 15:04:05")
	_, err = database.Exec("INSERT INTO Comments (Author, Date, Content, Post_Key) VALUES (?, ?, ?, ?)",
		author, date, content, postKey)
	return err
}

func getSearchResults(searchTerm string) (results []blogPost, err error) {
	database, err := sql.Open("sqlite3", dbName)
	if err != nil {
		return nil, err
	}

	rows, err := database.Query(`
		SELECT 
			(SELECT DisplayName FROM Authors WHERE Username = p.Author_Username) as Author,
			p.Key, p.Title, p.Content, p.Date
		FROM Posts p
		WHERE 
			p.Title LIKE ? OR p.Content LIKE ?
		ORDER BY p.Date DESC 
		LIMIT ?`, "%"+searchTerm+"%", "%"+searchTerm+"%", maxSearchResults)
	if err != nil {
		return nil, err
	}

	posts := make([]blogPost, 0)
	var post blogPost
	for rows.Next() {
		err = rows.Scan(
			&post.Author, &post.Key, &post.Title, &post.Content, &post.Date)
		if err != nil {
			return nil, err
		}
		post.Content = stripToSearchTerm(post.Content, searchTerm)
		posts = append(posts, post)
	}

	return posts, nil
}

func stripToSearchTerm(content, searchTerm string) (result string) {
	regex := regexp.MustCompile("<[^>]*>")
	result = regex.ReplaceAllString(content, "")
	loc := strings.Index(strings.ToLower(result), strings.ToLower(searchTerm))
	if loc == -1 {
		return ""
	}

	start, end := 0, len(result)-1
	if loc-searchStripPad > start {
		start = loc - searchStripPad
	}
	if loc+searchStripPad < end {
		end = loc + searchStripPad
	}

	return "..." + result[start:end] + "..."
}

func getYearMonthCounts() (years []yearSet, err error) {
	database, err := sql.Open("sqlite3", dbName)
	if err != nil {
		return nil, err
	}

	rows, err := database.Query(`
		SELECT 
			SUBSTR(Date, 0, 8) as Month, COUNT(Key) as Count 
		FROM 
			Posts 
		GROUP BY 
			Month 
		ORDER BY 
			Date`)
	if err != nil {
		return nil, err
	}

	years = make([]yearSet, 0)
	var month monthCount
	for rows.Next() {
		err = rows.Scan(&month.Month, &month.Count)
		if err != nil {
			return years, err
		}
		month.Year = month.Month[:4]
		monthNum := month.Month[5:]
		monthIndex, _ := strconv.Atoi(monthNum)
		month.Month = months[monthIndex]

		if len(years) == 0 || years[len(years)-1].Year != month.Year {
			year := yearSet{month.Year, []monthCount{month}}
			years = append(years, year)
		} else {
			years[len(years)-1].Months = append(years[len(years)-1].Months, month)
		}
	}

	return years, nil
}

func getStories() ([]blogPost, error) {
	database, err := sql.Open("sqlite3", dbName)
	if err != nil {
		return nil, err
	}

	rows, err := database.Query(`
		SELECT 
			(SELECT DisplayName FROM Authors WHERE Username = p.Author_Username) as Author,
			p.Key, p.Title, p.Date, p.IsStory, p.WordCount
		FROM Posts p 
		WHERE p.IsStory = 1
		ORDER BY p.Date DESC`)
	if err != nil {
		return nil, err
	}

	posts := make([]blogPost, 0)
	var post blogPost
	for rows.Next() {
		err = rows.Scan(
			&post.Author, &post.Key, &post.Title,
			&post.Date, &post.IsStory, &post.WordCount)
		if err != nil {
			return nil, err
		}
		posts = append(posts, post)
	}

	return posts, nil
}

func getPostsForMonth(month, year string) ([]blogPost, error) {
	database, err := sql.Open("sqlite3", dbName)
	if err != nil {
		return nil, err
	}

	monthToken := year + "-" + monthIndexes[month]

	rows, err := database.Query(`
		SELECT 
			(SELECT DisplayName FROM Authors WHERE Username = p.Author_Username) as Author,
			p.Key, p.Title, p.Content, p.Date, p.IsStory, p.WordCount, 
			(SELECT COUNT(*) FROM Comments WHERE Post_Key = p.Key) as CommentCount
		FROM Posts p
		WHERE SUBSTR(p.Date, 0, 8) = ?
		ORDER BY p.Date`, monthToken)
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

func getUser(username, password string) (user string, err error) {
	database, err := sql.Open("sqlite3", dbName)
	if err != nil {
		return "", err
	}

	row := database.QueryRow(`
		SELECT 
			Password
		FROM Authors
		WHERE Username = ?`, username)
	var hashAndSalt string
	err = row.Scan(&hashAndSalt)
	if err != nil {
		return "", err
	}

	split := strings.Index(hashAndSalt, ",")
	if split == -1 {
		return "", errors.New("invalid user password field")
	}

	toCheck := []byte(hashAndSalt[split+1:] + password)
	hasher := sha512.New384()
	hasher.Write(toCheck)
	result := base64.StdEncoding.EncodeToString(hasher.Sum(nil))

	if result != hashAndSalt[:split] {
		return "", errors.New("no match found")
	}

	return username, nil
}
