package handlers

import (
	"bytes"
	"io"
	"net/http"
	"regexp"
	"strconv"
	"strings"

	"github.com/ChrisPritchard/GrislyGrotto/internal/config"
	"github.com/ChrisPritchard/GrislyGrotto/internal/data"
	"github.com/ChrisPritchard/GrislyGrotto/pkg/aws"
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
		serverError(w, err)
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
		serverError(w, err)
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
		serverError(w, err)
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
		serverError(w, err)
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
		serverError(w, err)
		return
	}

	http.Redirect(w, r, "/post/"+key, http.StatusFound)
}

func contentHandler(w http.ResponseWriter, r *http.Request) {
	if r.Method == "GET" {
		tryGetContentFromStorage(w, r)
		return
	} else if r.Method != "POST" {
		http.NotFound(w, r)
		return
	} else if getCurrentUser(r) == nil {
		w.WriteHeader(http.StatusUnauthorized)
		return
	}
	tryUploadContentToStorage(w, r)
}

func tryGetContentFromStorage(w http.ResponseWriter, r *http.Request) {
	filename := r.URL.Path[len("/content/"):]
	if len(filename) == 0 {
		http.NotFound(w, r)
		return
	}

	lowerName := strings.ToLower(filename)
	validExtension := false
	for _, ext := range config.ValidUploadExtensions {
		if strings.HasSuffix(lowerName, ext) {
			validExtension = true
			break
		}
	}
	if !validExtension {
		badRequest(w, "bad file extension")
		return
	}

	bytes, exists, err := aws.RetrieveStorageFile(config.ContentStorageName, filename)
	if err != nil {
		serverError(w, err)
		return
	}
	if !exists {
		http.NotFound(w, r)
		return
	}

	headers := w.Header()
	headers.Set("Content-Type", "image/gif")
	headers.Set("X-Content-Type", "image/gif")
	w.Write(bytes)
}

func tryUploadContentToStorage(w http.ResponseWriter, r *http.Request) {
	filename := r.URL.Path[len("/content/"):]
	if len(filename) == 0 {
		http.NotFound(w, r)
		return
	}
	filename = strings.ToLower(filename)

	validExtension := false
	for _, ext := range config.ValidUploadExtensions {
		if strings.HasSuffix(filename, ext) {
			validExtension = true
		}
	}
	if !validExtension {
		badRequest(w, "bad file extension")
		return
	}

	file, fileHeader, err := r.FormFile("file")
	if err != nil {
		serverError(w, err)
		return
	}
	defer file.Close()

	if fileHeader.Size > config.MaxFileSize {
		badRequest(w, "file size exceeds maximum")
		return
	}

	buffer := bytes.NewBuffer(nil)
	if _, err := io.Copy(buffer, file); err != nil {
		badRequest(w, "Unable to read file")
		return
	}

	mimeType := http.DetectContentType(buffer.Bytes())
	if !strings.HasPrefix(mimeType, "image/") {
		badRequest(w, "File is not a valid image")
		return
	}

	err = aws.UploadStorageFile(config.ContentStorageName, filename, buffer)
	if err != nil {
		serverError(w, err)
		return
	}

	w.WriteHeader(http.StatusAccepted)
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
