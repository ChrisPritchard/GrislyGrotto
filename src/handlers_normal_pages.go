package main

import (
	"net/http"
	"strconv"
	"strings"
)

func latestPostsHandler(w http.ResponseWriter, r *http.Request) {
	if r.Method != "GET" {
		http.NotFound(w, r)
		return
	}

	page, notFirstPage := getPageFromQuery(r)
	posts, err := getLatestPosts(page)
	if err != nil {
		serverError(w, err)
		return
	}

	model := latestViewModel{notFirstPage, page - 1, page + 1, posts}
	renderView(w, r, model, "latest.html", "Latest Posts")
}

func getPageFromQuery(r *http.Request) (page int, notFirstPage bool) {
	pageParam, hasPage := r.URL.Query()["page"]
	page = 0
	notFirstPage = false
	if hasPage && len(pageParam[0]) > 0 {
		page, _ = strconv.Atoi(pageParam[0])
		if page < 0 {
			page = 0
		}
		if page != 0 {
			notFirstPage = true
		}
	}
	return page, notFirstPage
}

func singlePostHandler(w http.ResponseWriter, r *http.Request) {
	key := r.URL.Path[len("/post/"):]
	post, notFound, err := getSinglePost(key)
	if err != nil {
		serverError(w, err)
		return
	}

	if notFound {
		http.NotFound(w, r)
		return
	}

	ownBlog := currentUser == post.AuthorUsername

	if r.Method == "GET" {
		model := singleViewModel{post, ownBlog, true, ""}
		if len(post.Comments) >= maxCommentCount {
			model.CanComment = false
		}
		setBlockTime(r, "") // ensure comments can't be made immediately by spammers
		renderView(w, r, model, "single.html", post.Title)
		return
	}

	if r.Method != "POST" {
		http.NotFound(w, r)
		return
	}

	if len(post.Comments) >= maxCommentCount {
		badRequest(w, "max comments reached")
		return
	}

	commentError, err := createComment(r, post.Key)
	if err != nil {
		serverError(w, err)
		return
	}

	if commentError != "" {
		model := singleViewModel{post, ownBlog, true, commentError}
		renderView(w, r, model, "single.html", post.Title)
		return
	}

	http.Redirect(w, r, "/post/"+post.Key+"#comments", http.StatusFound)
}

func createComment(r *http.Request, postKey string) (commentError string, err error) {
	err = r.ParseForm()
	if err != nil {
		return "", err
	}

	author, content := r.Form["author"], r.Form["content"]
	if len(author) != 1 || len(author[0]) == 0 || len(content) != 1 || len(content[0]) == 0 || areDangerous(author[0], content[0]) {
		return "both author and content are required and must be safe values", nil
	}

	blockTime := getBlockTime(r, author[0])
	if blockTime > 0 {
		return "you may not make a comment for another " + strconv.Itoa(blockTime) + " seconds", nil
	}

	setBlockTime(r, author[0]) // primitive automated commenting protection

	err = addCommentToBlog(author[0], content[0], postKey)
	if err != nil {
		return "", err
	}
	return "", nil
}

func archivesHandler(w http.ResponseWriter, r *http.Request) {
	if r.Method != "GET" {
		http.NotFound(w, r)
		return
	}

	yearSets, err := getYearMonthCounts()
	if err != nil {
		serverError(w, err)
		return
	}

	stories, err := getStories()
	if err != nil {
		serverError(w, err)
		return
	}

	model := archivesViewModel{yearSets, stories}
	renderView(w, r, model, "archives.html", "Archives")
}

func monthHandler(w http.ResponseWriter, r *http.Request) {
	if r.Method != "GET" {
		http.NotFound(w, r)
		return
	}

	token := r.URL.Path[len("/archives/"):]
	split := strings.Index(token, "/")
	if len(token) == 0 || split == -1 {
		http.NotFound(w, r)
		return
	}

	month, year := token[:split], token[split+1:]
	if monthIndex(month) == -1 {
		http.NotFound(w, r)
		return
	}

	yearN, err := strconv.Atoi(year)
	if err != nil || yearN < 2006 || yearN > 2100 {
		http.NotFound(w, r)
		return
	}

	posts, err := getPostsForMonth(month, year)
	if err != nil {
		serverError(w, err)
		return
	}

	prevMonth, prevYear := getPrevMonth(month, yearN)
	nextMonth, nextYear := getNextMonth(month, yearN)
	model := monthViewModel{month, year, prevMonth, prevYear, nextMonth, nextYear, posts}
	renderView(w, r, model, "month.html", month+" "+year)
}

func getPrevMonth(month string, year int) (string, string) {
	if strings.ToLower(month) == "january" {
		return "December", strconv.Itoa(year - 1)
	}
	index := monthIndex(month)
	return months[index-1], strconv.Itoa(year)
}

func getNextMonth(month string, year int) (string, string) {
	if strings.ToLower(month) == "december" {
		return "January", strconv.Itoa(year + 1)
	}
	index := monthIndex(month)
	return months[index+1], strconv.Itoa(year)
}

func monthIndex(month string) int {
	for i, m := range months {
		if strings.ToLower(m) == strings.ToLower(month) {
			return i
		}
	}
	return -1
}

func searchHandler(w http.ResponseWriter, r *http.Request) {
	if r.Method != "GET" {
		http.NotFound(w, r)
		return
	}

	searchParam, hasSearch := r.URL.Query()["searchTerm"]
	if !hasSearch || len(searchParam[0]) == 0 {
		renderView(w, r, nil, "search.html", "Search")
		return
	}

	results, err := getSearchResults(searchParam[0])
	if err != nil {
		serverError(w, err)
		return
	}

	zeroResults := len(results) == 0
	renderView(w, r, searchViewModel{searchParam[0], zeroResults, results}, "search.html", "Search")
}

func aboutHandler(w http.ResponseWriter, r *http.Request) {
	if r.Method != "GET" {
		http.NotFound(w, r)
		return
	}

	renderView(w, r, nil, "about.html", "About")
}
