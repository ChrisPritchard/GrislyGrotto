package main

import (
	"encoding/base64"
	"net/http"
	"path/filepath"
	"regexp"
	"strconv"
	"strings"
	"time"
)

func ipOnly(ipAndPort string) string {
	portMarker := strings.LastIndex(ipAndPort, ":")
	if portMarker == -1 {
		return ipAndPort
	}
	return ipAndPort[:portMarker]
}

func getIP(r *http.Request) string {
	forwarded := r.Header.Get("x-forwarded-for") // case is normalised
	if forwarded == "" {
		return ipOnly(r.RemoteAddr)
	}
	return ipOnly(strings.Split(forwarded, ", ")[0])
}

func setBlockTime(r *http.Request, username string) {
	blocked[getIP(r)] = time.Now().Unix()
	blocked[username] = time.Now().Unix()
}

func cleanBlocked() {
	now := time.Now().Unix()
	for k, v := range blocked {
		if now-v > blockTime {
			delete(blocked, k)
		}
	}
}

func getBlockTime(r *http.Request, username string) int {
	now := time.Now().Unix()
	time1, time2 := now-blocked[getIP(r)], now-blocked[username]
	if time1 > blockTime && time2 > blockTime {
		return 0
	}
	cleanBlocked() // done on blocking to not affect ligitimate users (except commenters)
	if time2 < time1 {
		return blockTime - int(time2)
	}
	return blockTime - int(time1)
}

func embeddedStaticHandler(w http.ResponseWriter, r *http.Request) {
	file := r.URL.Path[len("/static/"):]
	ext := filepath.Ext(file)

	var fileContent string
	if content, exists := embeddedStatics[file]; exists {
		fileContent = content
	} else {
		http.NotFound(w, r)
		return
	}

	setMimeType(w, r)

	if ext == ".png" {
		bytes := make([]byte, base64.StdEncoding.DecodedLen(len(fileContent)))
		base64.StdEncoding.Decode(bytes, []byte(fileContent))
		w.Write(bytes)
	} else {
		w.Write([]byte(fileContent))
	}
}

func runtimeStaticHandler() http.Handler {
	server := http.FileServer(http.Dir("static"))
	fileHandler := http.HandlerFunc(func(w http.ResponseWriter, r *http.Request) {
		setMimeType(w, r)
		server.ServeHTTP(w, r)
	})

	return http.StripPrefix("/static/", fileHandler)
}

func setMimeType(w http.ResponseWriter, r *http.Request) {
	headers := w.Header()
	ext := filepath.Ext(r.URL.Path)

	switch ext {
	case ".css":
		headers.Set("Content-Type", "text/css")
	case ".js":
		headers.Set("Content-Type", "application/javascript")
	case ".png":
		headers.Set("Content-Type", "image/png")
	default:
		return
	}
}

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

	if currentUser == "" {
		unauthorised(w)
		return
	}

	success, err := tryDeleteComment(idN, currentUser) // only deletes if this is on a post the user owns
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

func deletePostHandler(w http.ResponseWriter, r *http.Request) {
	if r.Method != "POST" {
		http.NotFound(w, r)
		return
	}

	key := r.URL.Path[len("/delete-post/"):]
	if len(key) == 0 {
		http.NotFound(w, r)
		return
	}

	if currentUser == "" {
		unauthorised(w)
		return
	}

	post, notFound, err := getSinglePost(key)
	if err != nil {
		serverError(w, err)
		return
	}

	if notFound {
		http.NotFound(w, r)
		return
	}

	if post.AuthorUsername != currentUser {
		unauthorised(w)
		return
	}

	err = deletePost(key)
	if err != nil {
		serverError(w, err)
		return
	}

	http.Redirect(w, r, "/", http.StatusFound)
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

func loginHandler(w http.ResponseWriter, r *http.Request) {
	if r.Method == "GET" {
		renderView(w, r, loginViewModel{""}, "login.html", "Login")
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
		renderView(w, r, loginViewModel{"Both username and password are required"}, "login.html", "Login")
		return
	}

	blockTime := getBlockTime(r, username[0])
	if blockTime > 0 {
		renderView(w, r, loginViewModel{"Cannot make another attempt for another " + strconv.Itoa(blockTime) + " seconds"}, "login.html", "Login")
		return
	}

	user, err := getUser(username[0], password[0])
	if err != nil {
		setBlockTime(r, username[0])
		renderView(w, r, loginViewModel{"Invalid credentials"}, "login.html", "login")
		return
	}

	err = setEncryptedCookie("user", user, w)
	if err != nil {
		serverError(w, err)
		return
	}

	path := ""
	returnURI := r.URL.Query()["returnUrl"]
	if len(returnURI) > 0 {
		path = returnURI[0]
	}
	http.Redirect(w, r, "/"+path, http.StatusFound)
}

func editorHandler(w http.ResponseWriter, r *http.Request) {
	if r.Method != "GET" && r.Method != "POST" {
		http.NotFound(w, r)
		return
	}

	if currentUser == "" {
		unauthorised(w)
		return
	}

	key := r.URL.Path[len("/editor/"):]

	if len(key) == 0 {
		newPostHandler(w, r)
	} else {
		editPostHandler(w, r, key)
	}
}

func newPostHandler(w http.ResponseWriter, r *http.Request) {
	if r.Method == "GET" {
		model := editorViewModel{true, "", "", true, false, false, ""}
		renderView(w, r, model, "editor.html", "New Post")
		return
	}

	err := r.ParseForm()
	if err != nil {
		badRequest(w, "unable to parse form")
		return
	}

	titleF, contentF, renderModeF := r.Form["title"], r.Form["content"], r.Form["render-mode"]
	if len(titleF) != 1 || len(contentF) != 1 || len(renderModeF) != 1 {
		badRequest(w, "invalid form")
		return
	}

	isStory := len(r.Form["isStory"]) > 0
	isDraft := len(r.Form["isDraft"]) > 0
	title, content, isMarkdown := titleF[0], contentF[0], renderModeF[0] == "Markdown"

	if len(title) == 0 || len(content) == 0 {
		model := editorViewModel{true, title, content, isMarkdown, isStory, isDraft, "both title and content are required to be set"}
		renderView(w, r, model, "editor.html", "New Post")
		return
	}

	wordCount := calculateWordCount(content)
	if wordCount < minWordCount {
		model := editorViewModel{true, title, content, isMarkdown, isStory, isDraft, "the minimum word count for a post is " + strconv.Itoa(minWordCount)}
		renderView(w, r, model, "editor.html", "New Post")
		return
	}

	key := createPostKey(title)
	_, notFound, err := getSinglePost(key)
	if err != nil {
		serverError(w, err)
		return
	}

	if !notFound {
		model := editorViewModel{true, title, content, isMarkdown, isStory, isDraft, "a post with a similar title already exists"}
		renderView(w, r, model, "editor.html", "New Post")
		return
	}

	if isMarkdown {
		content = markdownToken + content
	}

	if isDraft {
		title = draftPrefix + title
	}

	err = createNewPost(key, title, content, isStory, wordCount, currentUser)
	if err != nil {
		serverError(w, err)
		return
	}

	http.Redirect(w, r, "/post/"+key, http.StatusFound)
}

func editPostHandler(w http.ResponseWriter, r *http.Request, key string) {
	post, notFound, err := getSinglePost(key)
	if err != nil {
		serverError(w, err)
		return
	}

	if notFound {
		http.NotFound(w, r)
		return
	}

	if post.AuthorUsername != currentUser {
		unauthorised(w)
		return
	}

	if r.Method == "GET" {
		postIsMarkdown := post.Content[:len(markdownToken)] == markdownToken
		content := post.Content
		if postIsMarkdown {
			content = content[len(markdownToken):]
		}
		postIsDraft := post.isDraft()
		if postIsDraft {
			post.Title = post.Title[len(draftPrefix):]
		}
		model := editorViewModel{false, post.Title, content, postIsMarkdown, post.IsStory, postIsDraft, ""}
		renderView(w, r, model, "editor.html", "Edit Post")
		return
	}

	err = r.ParseForm()
	if err != nil {
		badRequest(w, "unable to parse form")
		return
	}

	titleF, contentF, renderModeF := r.Form["title"], r.Form["content"], r.Form["render-mode"]
	if len(titleF) != 1 || len(contentF) != 1 || len(renderModeF) != 1 {
		badRequest(w, "invalid form")
		return
	}

	isStory := len(r.Form["isStory"]) > 0
	isDraft := len(r.Form["isDraft"]) > 0 && post.isDraft() // can only make a post a draft if it was already a draft
	title, content, isMarkdown := titleF[0], contentF[0], renderModeF[0] == "Markdown"

	if len(title) == 0 || len(content) == 0 {
		model := editorViewModel{true, title, content, isMarkdown, isStory, isDraft, "both title and content are required to be set"}
		renderView(w, r, model, "editor.html", "Edit Post")
		return
	}

	wordCount := calculateWordCount(content)
	if wordCount < minWordCount {
		model := editorViewModel{true, title, content, isMarkdown, isStory, isDraft, "the minimum word count for a post is " + strconv.Itoa(minWordCount)}
		renderView(w, r, model, "editor.html", "Edit Post")
		return
	}

	if isMarkdown {
		content = markdownToken + content
	}

	if isDraft {
		title = draftPrefix + title
	}

	err = updatePost(key, title, content, isStory, wordCount, post.isDraft() && !isDraft)
	if err != nil {
		serverError(w, err)
		return
	}

	http.Redirect(w, r, "/post/"+key, http.StatusFound)
}

func createPostKey(title string) string {
	clean := strings.Replace(strings.ToLower(title), " ", "-", -1)
	regex, _ := regexp.Compile("[^A-Za-z0-9 -]+")
	return string(regex.ReplaceAll([]byte(clean), []byte("")))
}

func calculateWordCount(content string) int {
	regex, _ := regexp.Compile("<[^>]*>")
	stripped := string(regex.ReplaceAll([]byte(content), []byte("")))
	return len(strings.Split(stripped, " "))
}

func themeHandler(w http.ResponseWriter, r *http.Request) {
	if r.Method != "POST" {
		http.NotFound(w, r)
		return
	}
	err := r.ParseForm()
	if err != nil {
		serverError(w, err)
	}
	setCookie("theme", r.Form["current-theme"][0], time.Now().Add(themeExpiry), w)
	http.Redirect(w, r, "/"+r.Form["return-path"][0], http.StatusFound)
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
