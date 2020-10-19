package main

import (
	"database/sql"
	"regexp"
	"strconv"
	"strings"
	"time"
)

func getLatestPosts(page int, currentUser *string) ([]blogPost, error) {
	rows, err := database.Query(`
		SELECT 
			(SELECT DisplayName FROM Authors WHERE Username = p.Author_Username) as Author,
			p.Author_Username as AuthorUsername,
			p.Key, p.Title, p.Content, p.Date, p.IsStory, p.WordCount,
			(SELECT COUNT(*) FROM Comments WHERE Post_Key = p.Key) as CommentCount
		FROM Posts p
		WHERE 
			(p.Title NOT LIKE ? OR p.Author_Username = ?)
		ORDER BY p.Date DESC 
		LIMIT ? OFFSET ?`, "%"+draftPrefix+"%", currentUser, pageLength, page*pageLength)
	if err != nil {
		return nil, err
	}
	defer rows.Close()

	posts := make([]blogPost, 0)
	var post blogPost
	for rows.Next() {
		err = rows.Scan(
			&post.Author, &post.AuthorUsername, &post.Key, &post.Title, &post.Content,
			&post.Date, &post.IsStory, &post.WordCount, &post.CommentCount)
		if err != nil {
			return nil, err
		}
		posts = append(posts, post)
	}

	return posts, nil
}

func getSinglePost(key string, currentUser *string) (post blogPost, notFound bool, err error) {
	row := database.QueryRow(`
		SELECT 
			(SELECT DisplayName FROM Authors WHERE Username = p.Author_Username) as Author,
			p.Author_Username, p.Title, p.Content, p.Date, p.IsStory
		FROM Posts p
		WHERE 
			Key = ?
			AND (p.Title NOT LIKE ? OR p.Author_Username = ?)`, key, "%"+draftPrefix+"%", currentUser)

	err = row.Scan(
		&post.Author, &post.AuthorUsername,
		&post.Title, &post.Content,
		&post.Date, &post.IsStory)

	if err != nil {
		if err == sql.ErrNoRows {
			return post, true, nil
		}
		return post, false, err
	}

	post.Key = key
	return post, false, err
}

func getPostWithComments(key string, currentUser *string, ownedComments map[int]interface{}) (post blogPost, notFound bool, err error) {
	post, notFound, err = getSinglePost(key, currentUser)
	if notFound || err != nil {
		return post, notFound, err
	}

	post.Comments = make([]blogComment, 0)
	rows, err := database.Query(`
		SELECT 
			Id, Author, Date, Content
		FROM Comments
		WHERE Post_Key = ?
		ORDER BY Date`, key)

	defer rows.Close()
	if err != nil {
		return post, notFound, err
	}

	var comment blogComment
	for rows.Next() {
		err = rows.Scan(
			&comment.ID, &comment.Author, &comment.Date, &comment.Content)
		if err != nil {
			return post, notFound, err
		}
		_, exists := ownedComments[comment.ID]
		comment.Owned = exists

		post.Comments = append(post.Comments, comment)
	}

	return post, false, nil
}

func addCommentToBlog(author, content, postKey string) (newID int64, err error) {
	date := time.Now().Format("2006-01-02 15:04:05")
	res, err := database.Exec(`
		INSERT INTO 
			Comments (Author, Date, Content, Post_Key) 
		VALUES (?, ?, ?, ?)`,
		author, date, content, postKey)
	if err != nil {
		return 0, err
	}
	newID, err = res.LastInsertId()
	return
}

func updateComment(id int, content string) error {
	_, err := database.Exec(`UPDATE Comments SET Content = ? WHERE Id = ?`, content, id)
	return err
}

func deleteComment(id int) error {
	_, err := database.Exec(`DELETE FROM Comments WHERE Id = ?`, id)
	return err
}

func deletePost(key string) (err error) {
	_, err = database.Exec(`
		DELETE FROM Posts
		WHERE Key = ?`,
		key)

	return err
}

func getSearchResults(searchTerm string, currentUser *string) (results []blogPost, err error) {
	rows, err := database.Query(`
		SELECT 
			(SELECT DisplayName FROM Authors WHERE Username = p.Author_Username) as Author,
			p.Key, p.Title, p.Content, p.Date
		FROM Posts p
		WHERE 
			(p.Title LIKE ? OR p.Content LIKE ?)
			AND (p.Title NOT LIKE ? OR p.Author_Username = ?)
		ORDER BY p.Date DESC 
		LIMIT ?`, "%"+searchTerm+"%", "%"+searchTerm+"%", "%"+draftPrefix+"%", currentUser, maxSearchResults)

	defer rows.Close()
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

func getYearMonthCounts(currentUser *string) (years []yearSet, err error) {
	rows, err := database.Query(`
		SELECT 
			SUBSTR(Date, 0, 8) as Month, COUNT(Key) as Count 
		FROM 
			Posts 
		WHERE
			(Title NOT LIKE ? OR Author_Username = ?)
		GROUP BY 
			Month 
		ORDER BY 
			Date DESC`, "%"+draftPrefix+"%", currentUser)
	defer rows.Close()
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

func getStories(currentUser *string) ([]blogPost, error) {
	rows, err := database.Query(`
		SELECT 
			(SELECT DisplayName FROM Authors WHERE Username = p.Author_Username) as Author,
			p.Key, p.Title, p.Date, p.IsStory, p.WordCount
		FROM Posts p 
		WHERE 
			p.IsStory = 1
			AND (p.Title NOT LIKE ? OR p.Author_Username = ?)
		ORDER BY p.Date DESC`, "%"+draftPrefix+"%", currentUser)
	defer rows.Close()
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

func getPostsForMonth(month, year string, currentUser *string) ([]blogPost, error) {
	monthToken := year + "-" + monthIndexes[month]

	rows, err := database.Query(`
		SELECT 
			(SELECT DisplayName FROM Authors WHERE Username = p.Author_Username) as Author,
			p.Author_Username as AuthorUsername,
			p.Key, p.Title, p.Content, p.Date, p.IsStory, p.WordCount, 
			(SELECT COUNT(*) FROM Comments WHERE Post_Key = p.Key) as CommentCount
		FROM Posts p
		WHERE 
			SUBSTR(p.Date, 0, 8) = ?
			AND (p.Title NOT LIKE ? OR p.Author_Username = ?)
		ORDER BY p.Date`, monthToken, "%"+draftPrefix+"%", currentUser)
	defer rows.Close()
	if err != nil {
		return nil, err
	}

	posts := make([]blogPost, 0)
	var post blogPost
	for rows.Next() {
		err = rows.Scan(
			&post.Author, &post.AuthorUsername, &post.Key, &post.Title, &post.Content,
			&post.Date, &post.IsStory, &post.WordCount, &post.CommentCount)
		if err != nil {
			return nil, err
		}
		posts = append(posts, post)
	}

	return posts, nil
}

func validateUser(username, password string) (valid bool, err error) {
	row := database.QueryRow(`
		SELECT 
			Password
		FROM Authors
		WHERE Username = ?`, username)

	var passwordHash string
	err = row.Scan(&passwordHash)
	if err != nil {
		return false, err
	}

	return compareWithArgonHash(password, passwordHash)
}

func insertOrUpdateUser(username, password, displayName string) error {
	passwordHash, err := generateArgonHash(passwordConfig, password)
	if err != nil {
		return err
	}

	res, err := database.Exec("UPDATE Authors SET Password = ?, DisplayName = ? WHERE Username = ?", passwordHash, displayName, username)

	if err != nil {
		return err
	}
	rows, err := res.RowsAffected()
	if err != nil || rows == 1 {
		return err
	}

	_, err = database.Exec("INSERT INTO Authors (Username, Password, DisplayName) VALUES (?, ?, ?)", username, passwordHash, displayName)
	return err
}

func getDisplayName(username string) (string, error) {
	row := database.QueryRow(`
		SELECT 
			DisplayName
		FROM Authors
		WHERE Username = ?`, username)

	var displayName string
	err := row.Scan(&displayName)
	if err != nil {
		return "", err
	}

	return displayName, nil
}

func createNewPost(key, title, content string, isStory bool, wordCount int, user string) (err error) {
	date := time.Now().Format("2006-01-02 15:04:05")
	_, err = database.Exec(`
		INSERT INTO 
			Posts (Author_Username, Key, Title, Date, Content, WordCount, IsStory) 
		VALUES (?, ?, ?, ?, ?, ?, ?)`,
		user, key, title, date, content, wordCount, isStory)
	return err
}

func updatePost(key, title, content string, isStory bool, wordCount int, updateDate bool) (err error) {
	if updateDate {
		date := time.Now().Format("2006-01-02 15:04:05")
		_, err = database.Exec(`
			UPDATE
				Posts 
			SET 
				Title = ?, Date = ?, Content = ?, WordCount = ?, IsStory = ? 
			WHERE
				Key = ?`,
			title, date, content, wordCount, isStory, key)
		return err
	}

	_, err = database.Exec(`
		UPDATE
			Posts 
		SET 
			Title = ?, Content = ?, WordCount = ?, IsStory = ? 
		WHERE
			Key = ?`,
		title, content, wordCount, isStory, key)
	return err
}
