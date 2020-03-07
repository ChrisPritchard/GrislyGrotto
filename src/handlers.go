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
	renderView(w, r, model, compiledViews.Latest)
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

	currentUser, err := readCookie("user", r)
	ownBlog := err == nil && currentUser == post.AuthorUsername

	if r.Method == "GET" {
		model := singleViewModel{post, ownBlog, true, ""}
		if len(post.Comments) >= maxCommentCount {
			model.CanComment = false
		}
		renderView(w, r, model, compiledViews.Single)
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

	model := singleViewModel{post, ownBlog, true, commentError}
	if len(post.Comments)+1 >= maxCommentCount {
		model.CanComment = false
	}

	renderView(w, r, model, compiledViews.Single)
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

	err = addCommentToBlog(author[0], content[0], postKey)
	if err != nil {
		return "", err
	}
	return "", nil
}

func deleteCommentHandler(w http.ResponseWriter, r *http.Request) {
	if r.Method != "POST" {
		http.NotFound(w, r)
		return
	}

	id := r.URL.Path[len("/delete-comment/"):]
	idN, err := strconv.Atoi(id)
	if err != nil {
		badRequest(w, "invalid comment id")
		return
	}

	user, err := readCookie("user", r)
	if err != nil {
		unauthorised(w)
		return
	}

	success, err := deleteComment(idN, user)
	if err != nil {
		serverError(w, err)
		return
	}

	if !success {
		unauthorised(w)
		return
	}

	postKey := r.URL.Query()["postKey"]
	returnURL := "/"

	if len(postKey) != 0 && len(postKey[0]) > 0 {
		returnURL = "/post/" + postKey[0] + "#comments"
	}

	http.Redirect(w, r, returnURL, http.StatusFound)
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
	renderView(w, r, model, compiledViews.Archives)
}

func monthHandler(w http.ResponseWriter, r *http.Request) {
	if r.Method != "GET" {
		http.NotFound(w, r)
		return
	}

	token := r.URL.Path[len("/month/"):]
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
	renderView(w, r, model, compiledViews.Month)
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
		renderView(w, r, nil, compiledViews.Search)
		return
	}

	results, err := getSearchResults(searchParam[0])
	if err != nil {
		serverError(w, err)
		return
	}

	zeroResults := len(results) == 0
	renderView(w, r, searchViewModel{searchParam[0], zeroResults, results}, compiledViews.Search)
}

func aboutHandler(w http.ResponseWriter, r *http.Request) {
	if r.Method != "GET" {
		http.NotFound(w, r)
		return
	}

	renderView(w, r, nil, compiledViews.About)
}

func loginHandler(w http.ResponseWriter, r *http.Request) {
	if r.Method == "GET" {
		renderView(w, r, loginViewModel{""}, compiledViews.Login)
		return
	}

	if r.Method != "POST" {
		http.NotFound(w, r)
		return
	}

	err := r.ParseForm()
	if err != nil {
		serverError(w, err)
		return
	}

	username, password := r.Form["username"], r.Form["password"]
	if len(username) != 1 || len(password) != 1 {
		renderView(w, r, loginViewModel{"both username and password are required"}, compiledViews.Login)
		return
	}

	user, err := getUser(username[0], password[0])
	if err != nil {
		renderView(w, r, loginViewModel{"invalid credentials"}, compiledViews.Login)
		return
	}

	err = setCookie("user", user, w)
	if err != nil {
		serverError(w, err)
		return
	}

	http.Redirect(w, r, "/", http.StatusFound)
}

func editorHandler(w http.ResponseWriter, r *http.Request) {
	if r.Method != "GET" && r.Method != "POST" {
		http.NotFound(w, r)
		return
	}

	user, err := readCookie("user", r)
	if err != nil {
		unauthorised(w)
		return
	}

	key := r.URL.Path[len("/editor/"):]
	if len(key) == 0 {
		newPostHandler(w, r, user)
	} else {
		editPostHandler(w, r, key, user)
	}
}

func newPostHandler(w http.ResponseWriter, r *http.Request, user string) {
	if r.Method == "GET" {
		model := editorViewModel{true, "", "", false, ""}
		renderView(w, r, model, compiledViews.Editor)
	}

	// todo create new
}

func editPostHandler(w http.ResponseWriter, r *http.Request, key string, user string) {
	post, notFound, err := getSinglePost(key)
	if err != nil {
		serverError(w, err)
		return
	}

	if notFound {
		http.NotFound(w, r)
		return
	}

	if post.AuthorUsername != user {
		unauthorised(w)
		return
	}

	if r.Method == "GET" {
		model := editorViewModel{false, post.Title, post.Content, post.IsStory, ""}
		renderView(w, r, model, compiledViews.Editor)
	}

	// todo update post
}

func areDangerous(values ...string) bool {
	for _, v := range values {
		if strings.ContainsAny(v, "<>") {
			return true
		}
	}
	return false
}

func serverError(w http.ResponseWriter, err error) {
	http.Error(w, err.Error(), http.StatusInternalServerError)
}

func badRequest(w http.ResponseWriter, message string) {
	http.Error(w, message, http.StatusBadRequest)
}

func unauthorised(w http.ResponseWriter) {
	http.Error(w, "unauthorised", http.StatusUnauthorized)
}
