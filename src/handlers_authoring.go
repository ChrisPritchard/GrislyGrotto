package main

import (
	"net/http"
	"regexp"
	"strconv"
	"strings"
)

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

func areDangerous(values ...string) bool {
	for _, v := range values {
		if strings.ContainsAny(v, "<>") {
			return true
		}
	}
	return false
}
