package main

import (
	"log"
	"net/http"

	_ "github.com/mattn/go-sqlite3"
)

func main() {
	views := compileViews()
	setupRoutes(views)

	log.Println("listening")
	log.Println(http.ListenAndServe(":3000", nil))
}

func setupRoutes(views views) {
	http.Handle("/static/",
		http.StripPrefix("/static/",
			http.FileServer(http.Dir("static"))))

	http.HandleFunc("/", latestPostsHandler(views))
	http.HandleFunc("/post/", singlePostHandler(views))
	http.HandleFunc("/archives", archivesHandler(views))
	http.HandleFunc("/month/", monthHandler(views))
	http.HandleFunc("/search", searchHandler(views))
	http.HandleFunc("/about", aboutHandler(views))
}
