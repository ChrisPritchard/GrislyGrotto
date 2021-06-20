package internal

import (
	"errors"
	"net/http"
	"sort"
	"strconv"
	"strings"
)

func getCommentAuthority(r *http.Request) map[int]interface{} {
	result := make(map[int]interface{})

	commentIDs, err := readEncryptedCookie("comments", commentAuthorityExpiry, r)
	if err != nil {
		return result
	}

	split := strings.Split(commentIDs, ",")
	for _, v := range split {
		id, _ := strconv.Atoi(v)
		result[id] = nil
	}

	return result
}

func setCommentAuthority(existing map[int]interface{}, newID int, w http.ResponseWriter) {
	ids := []int{newID}
	for id := range existing {
		ids = append(ids, id)
	}
	sort.Ints(ids)
	if len(ids) > maxOwnedComments {
		skip := len(ids) - maxOwnedComments
		ids = ids[:skip]
	}
	toStore := strconv.Itoa(ids[0])
	for i := 1; i < len(ids); i++ {
		toStore += "," + strconv.Itoa(ids[i])
	}

	setEncryptedCookie("comments", toStore, commentAuthorityExpiry, w)
}

func singlePostHandler(w http.ResponseWriter, r *http.Request) {
	key := r.URL.Path[len("/post/"):]
	currentUser := getCurrentUser(r)
	ownedComments := getCommentAuthority(r)

	post, notFound, err := getPostWithComments(key, currentUser, ownedComments)
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

	newID, commentError, err := createComment(r, post.Key)
	if err != nil {
		serverError(w, err)
		return
	}

	if commentError != "" {
		model := singleViewModel{post, ownBlog, true, commentError}
		renderView(w, r, model, "single.html", post.Title)
		return
	}

	setCommentAuthority(ownedComments, newID, w)

	http.Redirect(w, r, "/post/"+post.Key+"#comments", http.StatusFound)
}

func createComment(r *http.Request, postKey string) (newID int, commentError string, err error) {
	author, content := r.FormValue("author"), r.FormValue("content")
	if author == "" || content == "" {
		return 0, "both author and content are required", nil
	}

	if len(content) > maxCommentLength {
		return 0, "comment content exceeds max length of " + strconv.Itoa(maxCommentLength), nil
	}

	blockTime := getBlockTime(r, author)
	if blockTime > 0 {
		return 0, "you may not make a comment for another " + strconv.Itoa(blockTime) + " seconds", nil
	}

	setBlockTime(r, author) // primitive automated commenting protection

	id, err := addCommentToBlog(author, content, postKey)
	if err != nil {
		return 0, "", err
	}

	return int(id), "", nil
}

func rawCommentHandler(w http.ResponseWriter, r *http.Request) {
	if r.Method != "GET" {
		http.NotFound(w, r)
		return
	}

	id := r.URL.Path[len("/raw-comment/"):]
	idN, err := strconv.Atoi(id)
	if err != nil {
		badRequest(w, "invalid comment id")
		return
	}

	ownedComments := getCommentAuthority(r)
	if _, exists := ownedComments[idN]; !exists {
		unauthorised(w)
		return
	}

	commentSource, err := getCommentRaw(idN)
	if err != nil {
		serverError(w, errors.New("failed to retrieve comment with id "+id))
		return
	}

	w.Write([]byte(commentSource))
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

	ownedComments := getCommentAuthority(r)
	if _, exists := ownedComments[idN]; !exists {
		unauthorised(w)
		return
	}

	newContent := r.FormValue("content")
	if newContent == "" {
		badRequest(w, "content required")
		return
	}

	if len(newContent) > maxCommentLength {
		badRequest(w, "comment content exceeds max length of "+strconv.Itoa(maxCommentLength))
		return
	}

	err = updateComment(idN, newContent)
	if err != nil {
		serverError(w, err)
		return
	}

	w.WriteHeader(http.StatusAccepted)

	html := []byte(renderComment(newContent))
	w.Write(html)
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

	ownedComments := getCommentAuthority(r)
	if _, exists := ownedComments[idN]; !exists {
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
