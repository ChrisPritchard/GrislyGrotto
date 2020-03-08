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

	getConfig()    // setup globals from envs, flags, files
	compileViews() // parse template html files into compiled template vars
	setupRoutes()  // configure handlers for url fragments

	server := globalHandler(http.DefaultServeMux)

	log.Printf("The Grisly Grotto has started!\nlistening locally at port %s\n", listenURL)
	log.Println(http.ListenAndServe(fmt.Sprintf(listenURL), server))
}

func setupRoutes() {
	http.Handle("/static/",
		http.StripPrefix("/static/",
			http.FileServer(http.Dir("static"))))

	http.HandleFunc("/", latestPostsHandler) // note: this will catch any request not caught by the others
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

func globalHandler(h http.Handler) http.Handler {
	return http.HandlerFunc(func(w http.ResponseWriter, r *http.Request) {

		// set security headers
		headers := w.Header()
		headers.Set("X-Frame-Options", "SAMEORIGIN")
		headers.Set("X-XSS-Protection", "1; mode=block")
		headers.Set("X-Content-Type-Options", "nosniff")

		// read the current user once per request
		currentUser, _ = readCookie("user", r)

		h.ServeHTTP(w, r)
	})
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
