package main

import (
	"net/http"
	"strconv"
	"strings"
)

func singlePostHandler(w http.ResponseWriter, r *http.Request) {
	key := r.URL.Path[len("/post/"):]
	currentUser := getCurrentUser(r)
	post, notFound, err := getSinglePost(key, currentUser)
	if err != nil {
		serverError(w, err)
		return
	}

	if notFound {
		http.NotFound(w, r)
		return
	}

	ownBlog := currentUser != nil && *currentUser == post.AuthorUsername

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

	commentError, err := createComment(w, r, post.Key)
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

func createComment(w http.ResponseWriter, r *http.Request, postKey string) (commentError string, err error) {
	author, content := r.FormValue("author"), r.FormValue("content")
	if author == "" || content == "" {
		return "both author and content are required", nil
	}

	blockTime := getBlockTime(r, author)
	if blockTime > 0 {
		return "you may not make a comment for another " + strconv.Itoa(blockTime) + " seconds", nil
	}

	setBlockTime(r, author) // primitive automated commenting protection

	id, err := addCommentToBlog(author, content, postKey)
	if err != nil {
		return "", err
	}

	newCommentIDs := strconv.FormatInt(id, 10)
	commentIDs, err := readEncryptedCookie("comments", r)
	if err == nil {
		newCommentIDs += "," + commentIDs
	}
	setEncryptedCookie("comments", newCommentIDs, w)

	return "", nil
}

func hasCommentAuthority(commentID string, commentIDs string) bool {
	split := strings.Split(commentIDs, ",")
	for _, v := range split {
		if v == commentID {
			return true
		}
	}
	return false
}

func editCommentHandler(w http.ResponseWriter, r *http.Request) {
	if r.Method != "POST" {
		http.NotFound(w, r)
		return
	}

	id := r.URL.Path[len("/edit-comment/"):]
	idN, err := strconv.Atoi(id)
	if err != nil {
		badRequest(w, "invalid comment id")
		return
	}

	commentIDs, err := readEncryptedCookie("comments", r)
	if err != nil || !hasCommentAuthority(id, commentIDs) {
		unauthorised(w)
		return
	}

	newContent := r.FormValue("content")
	if newContent == "" {
		badRequest(w, "content required")
		return
	}

	err = updateComment(idN, newContent)
	if err != nil {
		serverError(w, err)
		return
	}

	w.WriteHeader(http.StatusAccepted)
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

	commentIDs, err := readEncryptedCookie("comments", r)
	if err != nil || !hasCommentAuthority(id, commentIDs) {
		unauthorised(w)
		return
	}

	err = deleteComment(idN)
	if err != nil {
		serverError(w, err)
		return
	}

	w.WriteHeader(http.StatusAccepted)
}
