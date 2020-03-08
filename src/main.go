package main

import (
	"flag"
	"fmt"
	"io/ioutil"
	"log"
	"net/http"
	"os"
	"strings"

	_ "github.com/mattn/go-sqlite3"
)

func main() {
	log.SetFlags(0)

	getConfig()
	compileViews()
	setupRoutes()

	log.Printf("The Grisly Grotto has started!\nlistening locally at port %s\n", listenURL)
	log.Println(http.ListenAndServe(fmt.Sprintf(listenURL), nil))
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
	}

	// args override env vars

	connArg := flag.String("db", "", "the sqlite connection string (e.g. ./grislygrotto.db)")
	urlArg := flag.String("url", "", "the url with port to listen to (e.g. :3000)")
	helpArg := flag.Bool("h", false, "print argument info (this page) and exit")
	flag.Parse()

	if *helpArg {
		log.Print("The Grisly Grotto can be started with the following args (these override env vars):\n\n")
		flag.PrintDefaults()
		log.Print("\nexiting (run without -h to start)")
		os.Exit(0)
	}

	if *connArg != "" {
		connectionString = *connArg
	}

	if *urlArg != "" {
		listenURL = *urlArg
	}

	// handles if url is fully qualified with scheme, which
	// is invalid for Go's ListenAndServe
	portStart := strings.LastIndex(listenURL, ":")
	if portStart > 0 {
		listenURL = listenURL[portStart:]
	} else if portStart < 0 {
		log.Fatal("invalid url specified - missing a :port")
	}

	var err error
	secret, err = ioutil.ReadFile("./.secret")
	if err != nil {
		log.Fatal(".secret file was not found or is not readable\nplease create a .secret file containing a 16 character secret for cookie encryption")
	}
}
