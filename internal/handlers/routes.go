package handlers

import "net/http"

func SetupRoutes() {
	http.HandleFunc("/static/", embeddedStaticHandler)

	http.HandleFunc("/", latestPostsHandler) // note: this will catch any request not caught by the others
	http.HandleFunc("/post/", singlePostHandler)
	http.HandleFunc("/raw-comment/", rawCommentHandler)
	http.HandleFunc("/edit-comment/", editCommentHandler)
	http.HandleFunc("/delete-comment/", deleteCommentHandler)
	http.HandleFunc("/delete-post/", deletePostHandler)
	http.HandleFunc("/archives", archivesHandler)
	http.HandleFunc("/archives/", monthHandler)
	http.HandleFunc("/search/", searchHandler)
	http.HandleFunc("/about", aboutHandler)
	http.HandleFunc("/login", loginHandler)
	http.HandleFunc("/logout", logoutHandler)
	http.HandleFunc("/profile-image/", profileImageHandler)
	http.HandleFunc("/account-details", accountDetailsHandler)
	http.HandleFunc("/editor/", editorHandler)
	http.HandleFunc("/save-theme", themeHandler)
	http.HandleFunc("/content/", contentHandler)
	http.HandleFunc("/backup/posts", postsBackupHandler)
	http.HandleFunc("/backup/content", contentBackupHandler)
}
