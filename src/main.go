package main

import (
	"encoding/base64"
	"flag"
	"io/ioutil"
	"log"
	"net/http"
	"os"
	"path/filepath"
	"strings"

	_ "github.com/mattn/go-sqlite3"
)

//go:generate go run static/embedStatic.go
//go:generate go run templates/embedTemplates.go

func main() {
	log.SetFlags(0)

	getConfig()    // setup globals from envs, flags, files
	compileViews() // parse template html files into compiled template vars
	setupRoutes()  // configure handlers for url fragments
	loadStatics()  // ensure static content is in place

	server := globalHandler(http.DefaultServeMux)

	log.Printf("The Grisly Grotto has started!\nlistening locally at port %s\n", listenURL)
	log.Println(http.ListenAndServe(listenURL, server))
}

func setupRoutes() {
	http.HandleFunc("/static/", embeddedStaticHandler)

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

// this gets populated via the generated static.go
var embeddedStatics = make(map[string]string)

func embeddedStaticHandler(w http.ResponseWriter, r *http.Request) {
	headers := w.Header()
	file := r.URL.Path[len("/static/"):]
	ext := filepath.Ext(file)

	var fileContent string
	if content, exists := embeddedStatics[file]; exists {
		fileContent = content
	} else {
		http.NotFound(w, r)
		return
	}

	switch ext {
	case ".css":
		headers.Set("Content-Type", "text/css")
		w.Write([]byte(fileContent))
	case ".js":
		headers.Set("Content-Type", "application/javascript")
		w.Write([]byte(fileContent))
	case ".png":
		headers.Set("Content-Type", "image/png")
		bytes := make([]byte, base64.StdEncoding.DecodedLen(len(fileContent)))
		base64.StdEncoding.Decode(bytes, []byte(fileContent))
		w.Write(bytes)
	default:
		w.Write([]byte(fileContent))
	}
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
	flag.Parse()

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
