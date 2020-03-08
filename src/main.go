package main

import (
	"fmt"
	"io/ioutil"
	"log"
	"net/http"
	"os"
	"strings"

	_ "github.com/mattn/go-sqlite3"
)

var secret []byte
var compiledViews views
var connectionString string
var listenURL string

func main() {
	var err error
	secret, err = ioutil.ReadFile("./.secret")
	if err != nil {
		log.Fatal(err)
	}

	getConfig()
	compileViews()
	setupRoutes()

	log.Printf("listening locally at %s\n", listenURL)
	log.Println(http.ListenAndServe(fmt.Sprintf(listenURL), nil))
}

func getConfig() {
	// env names come from prior .NET version of the site.
	// kept the same to make server setup simpler/lulzy
	connectionString = os.Getenv("ConnectionStrings__default")
	if connectionString == "" {
		connectionString = defaultConnectionString
	}
	listenURL = os.Getenv("ASPNETCORE_URLS")
	if listenURL == "" {
		listenURL = defaultListenAddr
	} else {
		portStart := strings.LastIndex(listenURL, ":")
		if portStart > 0 {
			listenURL = listenURL[portStart:]
		}
	}
}

func setupRoutes() {
	http.Handle("/static/",
		http.StripPrefix("/static/",
			http.FileServer(http.Dir("static"))))

	http.HandleFunc("/", latestPostsHandler)
	http.HandleFunc("/post/", singlePostHandler)
	http.HandleFunc("/delete-comment/", deleteCommentHandler)
	http.HandleFunc("/delete-post/", deletePostHandler)
	http.HandleFunc("/archives", archivesHandler)
	http.HandleFunc("/month/", monthHandler)
	http.HandleFunc("/search/", searchHandler)
	http.HandleFunc("/about", aboutHandler)
	http.HandleFunc("/login", loginHandler)
	http.HandleFunc("/editor/", editorHandler)
}
