package handlers

import (
	"net/http"
	"regexp"
	"strconv"
	"strings"

	"github.com/ChrisPritchard/GrislyGrotto/internal/config"
	"github.com/ChrisPritchard/GrislyGrotto/internal/data"
)

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

	currentUser := getCurrentUser(r)
	if currentUser == nil {
		unauthorised(w)
		return
	}

	post, notFound, err := data.GetSinglePost(key, currentUser)
	if err != nil {
		serverError(w, r, err)
		return
	}

	if notFound {
		http.NotFound(w, r)
		return
	}

	if post.AuthorUsername != *currentUser {
		unauthorised(w)
		return
	}

	err = data.DeletePost(key)
	if err != nil {
		serverError(w, r, err)
		return
	}

	http.Redirect(w, r, "/", http.StatusFound)
}

func editorHandler(w http.ResponseWriter, r *http.Request) {
	if r.Method != "GET" && r.Method != "POST" {
		http.NotFound(w, r)
		return
	}

	currentUser := getCurrentUser(r)
	if currentUser == nil {
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
		model := editorViewModel{true, "", "", false, false, ""}
		renderView(w, r, model, "editor.html", "New Post")
		return
	}

	currentUser := getCurrentUser(r)
	if currentUser == nil {
		unauthorised(w)
		return
	}

	isStory := r.FormValue("isStory") != ""
	isDraft := r.FormValue("isDraft") != ""
	title := r.FormValue("title")
	content := r.FormValue("content")

	if title == "" || content == "" {
		model := editorViewModel{true, title, content, isStory, isDraft, "both title and content are required to be set"}
		renderView(w, r, model, "editor.html", "New Post")
		return
	}

	wordCount := calculateWordCount(content)
	if wordCount < config.MinWordCount {
		model := editorViewModel{true, title, content, isStory, isDraft, "the minimum word count for a post is " + strconv.Itoa(config.MinWordCount)}
		renderView(w, r, model, "editor.html", "New Post")
		return
	}

	key := createPostKey(title)
	_, notFound, err := data.GetSinglePost(key, currentUser)
	if err != nil {
		serverError(w, r, err)
		return
	}

	if !notFound {
		model := editorViewModel{true, title, content, isStory, isDraft, "a post with a similar title already exists"}
		renderView(w, r, model, "editor.html", "New Post")
		return
	}

	if isDraft {
		title = config.DraftPrefix + title
	}

	err = data.CreateNewPost(key, title, content, isStory, wordCount, *currentUser)
	if err != nil {
		serverError(w, r, err)
		return
	}

	http.Redirect(w, r, "/post/"+key, http.StatusFound)
}

func editPostHandler(w http.ResponseWriter, r *http.Request, key string) {
	currentUser := getCurrentUser(r)
	if currentUser == nil {
		unauthorised(w)
		return
	}

	post, notFound, err := data.GetSinglePost(key, currentUser)
	if err != nil {
		serverError(w, r, err)
		return
	}

	if notFound {
		http.NotFound(w, r)
		return
	}

	if post.AuthorUsername != *currentUser {
		unauthorised(w)
		return
	}

	if r.Method == "GET" {
		postIsDraft := post.IsDraft()
		if postIsDraft {
			post.Title = post.Title[len(config.DraftPrefix):]
		}
		model := editorViewModel{false, post.Title, post.Content, post.IsStory, postIsDraft, ""}
		renderView(w, r, model, "editor.html", "Edit Post")
		return
	}

	isStory := r.FormValue("isStory") != ""
	isDraft := r.FormValue("isDraft") != ""
	title := r.FormValue("title")
	content := r.FormValue("content")

	if title == "" || content == "" {
		model := editorViewModel{false, title, content, isStory, isDraft, "both title and content are required to be set"}
		renderView(w, r, model, "editor.html", "Edit Post")
		return
	}

	if len(title) > config.MaxTitleLength {
		model := editorViewModel{false, title, content, isStory, isDraft, "title exceeds max title length of " + strconv.Itoa(config.MaxTitleLength)}
		renderView(w, r, model, "editor.html", "Edit Post")
		return
	}

	wordCount := calculateWordCount(content)
	if wordCount < config.MinWordCount {
		model := editorViewModel{false, title, content, isStory, isDraft, "the minimum word count for a post is " + strconv.Itoa(config.MinWordCount)}
		renderView(w, r, model, "editor.html", "Edit Post")
		return
	}

	if isDraft {
		title = config.DraftPrefix + title
	}

	err = data.UpdatePost(key, title, content, isStory, wordCount, post.IsDraft() && !isDraft)
	if err != nil {
		serverError(w, r, err)
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
