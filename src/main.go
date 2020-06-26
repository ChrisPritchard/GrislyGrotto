package main

import (
	"flag"
	"io/ioutil"
	"log"
	"net/http"
	"strings"

	_ "github.com/mattn/go-sqlite3"
)

//go:generate go run static/embedStatic.go
//go:generate go run templates/embedTemplates.go

func main() {
	log.SetFlags(0)

	getConfig() // setup globals from cmd line flags and files

	setupRoutes() // configure handlers for url fragments

	if !isDevelopment {
		loadTemplates() // create the template html map
		loadStatics()   // ensure static content is in place
	}

	server := globalHandler(http.DefaultServeMux)

	log.Printf("The Grisly Grotto has started!\nlistening locally at port %s\n", listenURL)
	if isDevelopment {
		log.Print("Running in DEVELOPMENT mode\n")
	}
	log.Println(http.ListenAndServe(listenURL, server))
}

func setupRoutes() {
	if isDevelopment {
		http.Handle("/static/", runtimeStaticHandler())
	} else {
		http.HandleFunc("/static/", embeddedStaticHandler)
	}

	http.HandleFunc("/", latestPostsHandler) // note: this will catch any request not caught by the others
	http.HandleFunc("/post/", singlePostHandler)
	http.HandleFunc("/delete-comment/", deleteCommentHandler)
	http.HandleFunc("/delete-post/", deletePostHandler)
	http.HandleFunc("/archives", archivesHandler)
	http.HandleFunc("/archives/", monthHandler)
	http.HandleFunc("/search/", searchHandler)
	http.HandleFunc("/about", aboutHandler)
	http.HandleFunc("/login", loginHandler)
	http.HandleFunc("/editor/", editorHandler)
	http.HandleFunc("/save-theme", themeHandler)
}

func globalHandler(h http.Handler) http.Handler {
	return http.HandlerFunc(func(w http.ResponseWriter, r *http.Request) {

		// set security headers
		headers := w.Header()
		headers.Set("X-Frame-Options", "SAMEORIGIN")
		headers.Set("X-XSS-Protection", "1; mode=block")
		headers.Set("X-Content-Type-Options", "nosniff")

		csp := "default-src 'none';"
		csp += "script-src 'self';"
		csp += "connect-src 'self';"
		csp += "img-src 'self' https://grislygrotto.blob.core.windows.net;"
		csp += "style-src 'self';"
		csp += "frame-src 'self' *.youtube.com;"
		headers.Set("Content-Security-Policy", csp)

		// read the current user once per request
		currentUser, _ = readEncryptedCookie("user", r)
		if currentUser != "" {
			// refresh the cookie
			setEncryptedCookie("user", currentUser, w)
		}

		h.ServeHTTP(w, r)
	})
}

func getConfig() {

	connectionString = defaultConnectionString
	listenURL = defaultListenAddr

	connArg := flag.String("db", "", "the sqlite connection string\n\tdefaults to "+defaultConnectionString)
	urlArg := flag.String("url", "", "the url with port to listen to\n\tdefaults to "+defaultListenAddr)
	envArg := flag.Bool("dev", false, "sets to run in 'dev' mode\n\tif set resources are loaded on request rather than embedded")
	flag.Parse()

	if *connArg != "" {
		connectionString = *connArg
	}

	if *urlArg != "" {
		listenURL = *urlArg
	}

	isDevelopment = *envArg

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
