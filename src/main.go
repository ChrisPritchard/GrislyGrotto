package main

import (
	"fmt"
	"log"
	"net/http"

	_ "github.com/mattn/go-sqlite3"
)

func main() {
	views := compileViews()
	setupRoutes(views)

	log.Printf("listening locally on port :%d\n", listenPort)
	log.Println(http.ListenAndServe(fmt.Sprintf(":%d", listenPort), nil))
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
