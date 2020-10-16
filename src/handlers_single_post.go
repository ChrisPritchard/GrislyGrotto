package main

import (
	"net/http"
	"strconv"
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
	author, content := r.FormValue("author"), r.FormValue("content")
	if author == "" || content == "" {
		return "both author and content are required", nil
	}

	blockTime := getBlockTime(r, author)
	if blockTime > 0 {
		return "you may not make a comment for another " + strconv.Itoa(blockTime) + " seconds", nil
	}

	setBlockTime(r, author) // primitive automated commenting protection

	err = addCommentToBlog(author, content, postKey)
	if err != nil {
		return "", err
	}
	return "", nil
}

func editCommentHandler(w http.ResponseWriter, r *http.Request) {
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

	// todo check comment cookie or belongs to current user post

	currentUser := getCurrentUser(r)
	if currentUser == nil {
		unauthorised(w)
		return
	}

	success, err := tryDeleteComment(idN, *currentUser) // only deletes if this is on a post the user owns
	if err != nil {
		serverError(w, err)
		return
	}

	if !success {
		unauthorised(w)
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

	// todo check comment cookie or belongs to current user post

	currentUser := getCurrentUser(r)
	if currentUser == nil {
		unauthorised(w)
		return
	}

	success, err := tryDeleteComment(idN, *currentUser) // only deletes if this is on a post the user owns
	if err != nil {
		serverError(w, err)
		return
	}

	if !success {
		unauthorised(w)
		return
	}

	w.WriteHeader(http.StatusAccepted)
}
