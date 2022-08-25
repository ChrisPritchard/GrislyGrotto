package data

import (
	"database/sql"
	"regexp"
	"strconv"
	"strings"

	"github.com/ChrisPritchard/GrislyGrotto/internal/config"
	"github.com/ChrisPritchard/GrislyGrotto/pkg/argon2"
)

type StreamedBlogPost struct {
	Post  BlogPost
	Error error
}

func GetAllPostsAsync(out chan<- StreamedBlogPost) {
	defer close(out)

	rows, err := config.Database.Query("SELECT p.Author_Username as AuthorUsername, p.Key, p.Title, p.Content, p.Date, p.IsStory, p.WordCount FROM Posts p")
	if err != nil {
		out <- StreamedBlogPost{Error: err}
		return
	}
	defer rows.Close()

	var post BlogPost
	for rows.Next() {
		err = rows.Scan(
			&post.AuthorUsername, &post.Key, &post.Title, &post.Content, &post.Date, &post.IsStory, &post.WordCount)
		if err != nil {
			out <- StreamedBlogPost{Error: err}
			return
		}

		comments, err := commentsForPost(post.Key)
		if err != nil {
			out <- StreamedBlogPost{Error: err}
			return
		}
		post.Comments = comments

		out <- StreamedBlogPost{Post: post}
	}
}

func GetLatestPosts(page int, currentUser *string) ([]BlogPost, error) {
	rows, err := config.Database.Query(`
		SELECT 
			(SELECT DisplayName FROM Authors WHERE Username = p.Author_Username) as Author,
			p.Author_Username as AuthorUsername,
			p.Key, p.Title, p.Content, p.Date, p.IsStory, p.WordCount,
			(SELECT COUNT(*) FROM Comments WHERE Post_Key = p.Key) as CommentCount
		FROM Posts p
		WHERE 
			(p.Title NOT LIKE ? OR p.Author_Username = ?)
		ORDER BY p.Date DESC 
		LIMIT ? OFFSET ?`, "%"+config.DraftPrefix+"%", currentUser, config.PageLength, page*config.PageLength)
	if err != nil {
		return nil, err
	}
	defer rows.Close()

	posts := make([]BlogPost, 0)
	var post BlogPost
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

func GetSinglePost(key string, currentUser *string) (post BlogPost, notFound bool, err error) {
	row := config.Database.QueryRow(`
		SELECT 
			(SELECT DisplayName FROM Authors WHERE Username = p.Author_Username) as Author,
			p.Author_Username, p.Title, p.Content, p.Date, p.IsStory
		FROM Posts p
		WHERE 
			Key = ?
			AND (p.Title NOT LIKE ? OR p.Author_Username = ?)`, key, "%"+config.DraftPrefix+"%", currentUser)

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

func GetPostWithComments(key string, currentUser *string, ownedComments map[int]interface{}) (post BlogPost, notFound bool, err error) {
	post, notFound, err = GetSinglePost(key, currentUser)
	if notFound || err != nil {
		return post, notFound, err
	}

	comments, err := commentsForPost(key)
	if err != nil {
		return post, notFound, err
	}

	post.Comments = comments
	for i := range post.Comments {
		_, exists := ownedComments[post.Comments[i].ID]
		post.Comments[i].Owned = exists
	}

	return post, false, nil
}

func commentsForPost(key string) ([]BlogComment, error) {
	comments := make([]BlogComment, 0)
	rows, err := config.Database.Query(`
		SELECT 
			Id, Author, Date, Content
		FROM Comments
		WHERE Post_Key = ?
		ORDER BY Date`, key)
	if err != nil {
		return nil, err
	}

	defer rows.Close()

	var comment BlogComment
	for rows.Next() {
		err = rows.Scan(
			&comment.ID, &comment.Author, &comment.Date, &comment.Content)
		if err != nil {
			return nil, err
		}

		comments = append(comments, comment)
	}

	return comments, nil
}

func AddCommentToBlog(author, content, postKey string) (newID int64, err error) {
	date := config.CurrentTime().Format("2006-01-02 15:04:05")
	res, err := config.Database.Exec(`
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

func GetCommentRaw(id int) (string, error) {
	row := config.Database.QueryRow(`SELECT Content FROM Comments WHERE Id = ?`, id)

	var content string
	err := row.Scan(&content)
	if err != nil {
		return "", err
	}

	return content, nil
}

func UpdateComment(id int, content string) error {
	_, err := config.Database.Exec(`UPDATE Comments SET Content = ? WHERE Id = ?`, content, id)
	return err
}

func DeleteComment(id int) error {
	_, err := config.Database.Exec(`DELETE FROM Comments WHERE Id = ?`, id)
	return err
}

func DeletePost(key string) (err error) {
	_, err = config.Database.Exec(`
		DELETE FROM Posts
		WHERE Key = ?`,
		key)

	return err
}

func GetSearchResults(searchTerm string, currentUser *string) (results []BlogPost, err error) {
	rows, err := config.Database.Query(`
		SELECT 
			(SELECT DisplayName FROM Authors WHERE Username = p.Author_Username) as Author,
			p.Key, p.Title, p.Content, p.Date
		FROM Posts p
		WHERE 
			(p.Title LIKE ? OR p.Content LIKE ?)
			AND (p.Title NOT LIKE ? OR p.Author_Username = ?)
		ORDER BY p.Date DESC 
		LIMIT ?`, "%"+searchTerm+"%", "%"+searchTerm+"%", "%"+config.DraftPrefix+"%", currentUser, config.MaxSearchResults)

	if err != nil {
		return nil, err
	}

	defer rows.Close()

	posts := make([]BlogPost, 0)
	var post BlogPost
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
	if loc-config.SearchStripPad > start {
		start = loc - config.SearchStripPad
	}
	if loc+config.SearchStripPad < end {
		end = loc + config.SearchStripPad
	}

	return "..." + result[start:end] + "..."
}

func GetYearMonthCounts(currentUser *string) (years []YearSet, err error) {
	rows, err := config.Database.Query(`
		SELECT 
			SUBSTR(Date, 0, 8) as Month, COUNT(Key) as Count 
		FROM 
			Posts 
		WHERE
			(Title NOT LIKE ? OR Author_Username = ?)
		GROUP BY 
			Month 
		ORDER BY 
			Date DESC`, "%"+config.DraftPrefix+"%", currentUser)
	if err != nil {
		return nil, err
	}

	defer rows.Close()

	years = make([]YearSet, 0)
	var month MonthCount
	for rows.Next() {
		err = rows.Scan(&month.Month, &month.Count)
		if err != nil {
			return years, err
		}
		month.Year = month.Month[:4]
		monthNum := month.Month[5:]
		monthIndex, _ := strconv.Atoi(monthNum)
		month.Month = config.Months[monthIndex]

		if len(years) == 0 || years[len(years)-1].Year != month.Year {
			year := YearSet{month.Year, []MonthCount{month}}
			years = append(years, year)
		} else {
			years[len(years)-1].Months = append(years[len(years)-1].Months, month)
		}
	}

	return years, nil
}

func GetStories(currentUser *string) ([]BlogPost, error) {
	rows, err := config.Database.Query(`
		SELECT 
			(SELECT DisplayName FROM Authors WHERE Username = p.Author_Username) as Author,
			p.Key, p.Title, p.Date, p.IsStory, p.WordCount
		FROM Posts p 
		WHERE 
			p.IsStory = 1
			AND (p.Title NOT LIKE ? OR p.Author_Username = ?)
		ORDER BY p.Date DESC`, "%"+config.DraftPrefix+"%", currentUser)
	if err != nil {
		return nil, err
	}

	defer rows.Close()

	posts := make([]BlogPost, 0)
	var post BlogPost
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

func GetPostsForMonth(month, year string, currentUser *string) ([]BlogPost, error) {
	monthToken := year + "-" + config.MonthIndexes[month]

	rows, err := config.Database.Query(`
		SELECT 
			(SELECT DisplayName FROM Authors WHERE Username = p.Author_Username) as Author,
			p.Author_Username as AuthorUsername,
			p.Key, p.Title, p.Content, p.Date, p.IsStory, p.WordCount, 
			(SELECT COUNT(*) FROM Comments WHERE Post_Key = p.Key) as CommentCount
		FROM Posts p
		WHERE 
			SUBSTR(p.Date, 0, 8) = ?
			AND (p.Title NOT LIKE ? OR p.Author_Username = ?)
		ORDER BY p.Date`, monthToken, "%"+config.DraftPrefix+"%", currentUser)

	if err != nil {
		return nil, err
	}

	defer rows.Close()

	posts := make([]BlogPost, 0)
	var post BlogPost
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

func ValidateUser(username, password string) (valid bool, err error) {
	row := config.Database.QueryRow(`
		SELECT 
			Password
		FROM Authors
		WHERE Username = ?`, username)

	var passwordHash string
	err = row.Scan(&passwordHash)
	if err != nil {
		return false, err
	}

	return argon2.CompareWithArgonHash(password, passwordHash)
}

func InsertOrUpdateUser(username, password, displayName string) error {
	var res sql.Result
	var err error
	var passwordHash string

	if password != "" {
		passwordHash, err = argon2.GenerateArgonHash(config.PasswordConfig, password)
		if err != nil {
			return err
		}
	}

	if password == "" {
		res, err = config.Database.Exec("UPDATE Authors SET DisplayName = ? WHERE Username = ?", displayName, username)
	} else if displayName != "" {
		res, err = config.Database.Exec("UPDATE Authors SET Password = ?, DisplayName = ? WHERE Username = ?", passwordHash, displayName, username)
	} else {
		res, err = config.Database.Exec("UPDATE Authors SET Password = ? WHERE Username = ?", passwordHash, username)
	}

	if err != nil {
		return err
	}
	rows, err := res.RowsAffected()
	if err != nil || rows == 1 {
		return err
	}

	_, err = config.Database.Exec("INSERT INTO Authors (Username, Password, DisplayName) VALUES (?, ?, ?)", username, passwordHash, displayName)
	return err
}

func GetDisplayName(username string) (string, error) {
	row := config.Database.QueryRow(`
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

func CreateNewPost(key, title, content string, isStory bool, wordCount int, user string) (err error) {
	date := config.CurrentTime().Format("2006-01-02 15:04:05")
	_, err = config.Database.Exec(`
		INSERT INTO 
			Posts (Author_Username, Key, Title, Date, Content, WordCount, IsStory) 
		VALUES (?, ?, ?, ?, ?, ?, ?)`,
		user, key, title, date, content, wordCount, isStory)
	return err
}

func UpdatePost(key, title, content string, isStory bool, wordCount int, updateDate bool) (err error) {
	if updateDate {
		date := config.CurrentTime().Format("2006-01-02 15:04:05")
		_, err = config.Database.Exec(`
			UPDATE
				Posts 
			SET 
				Title = ?, Date = ?, Content = ?, WordCount = ?, IsStory = ? 
			WHERE
				Key = ?`,
			title, date, content, wordCount, isStory, key)
		return err
	}

	_, err = config.Database.Exec(`
		UPDATE
			Posts 
		SET 
			Title = ?, Content = ?, WordCount = ?, IsStory = ? 
		WHERE
			Key = ?`,
		title, content, wordCount, isStory, key)
	return err
}
