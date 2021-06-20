package internal

import (
	"context"
	"crypto/rand"
	"database/sql"
	"flag"
	"io/ioutil"
	"log"
	"net/http"
	"strings"
)

func StartServer() {
	proceed := getConfig() // setup globals from cmd line flags and files
	if !proceed {
		return
	}

	setupRoutes()

	server := globalHandler(http.DefaultServeMux)

	log.Printf("The Grisly Grotto has started!\nlistening locally at port %s\n", listenURL)
	if isDevelopment {
		log.Print("Running in DEVELOPMENT mode\n")
	}
	log.Println(http.ListenAndServe(listenURL, server))
}

func getConfig() bool {
	connectionString = defaultConnectionString
	listenURL = defaultListenAddr
	contentStorageName = defaultStorageName

	connArg := flag.String("db", "", "the sqlite connection string\n\tdefaults to "+defaultConnectionString)
	urlArg := flag.String("url", "", "the url with port to listen to\n\tdefaults to "+defaultListenAddr)
	storageArg := flag.String("storage", "", "the target storage bucket/container for user content\n\tdefaults to "+defaultStorageName)
	envArg := flag.Bool("dev", false, "sets to run in 'dev' mode\n\tif set resources are loaded on request rather than embedded")
	setAuthorArg := flag.Bool("setauthor", false, "creates or updates a login account, then exits\nshould be followed by [username] [password] [displayname]")
	flag.Parse()

	if *connArg != "" {
		connectionString = *connArg
	}

	db, err := sql.Open("sqlite3", connectionString) // db is closed by app close
	if err != nil {
		log.Fatal(err)
	}
	database = db

	if *setAuthorArg {
		parts := flag.Args()
		if len(parts) != 3 {
			flag.PrintDefaults()
			return false
		}
		err := insertOrUpdateUser(parts[0], parts[1], parts[2])
		if err != nil {
			log.Fatal(err)
		}
		log.Println("user created or updated successfully")
		return false
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

	s, err := ioutil.ReadFile("./.secret")
	if err != nil || len(s) != 16 {
		bytes := make([]byte, 16)
		rand.Read(bytes)
		s = bytes
	}
	secret = s

	if *storageArg != "" {
		contentStorageName = *storageArg
	}

	isDevelopment = *envArg
	return true
}

func setupRoutes() {
	if isDevelopment {
		http.Handle("/static/", runtimeStaticHandler())
	} else {
		http.HandleFunc("/static/", embeddedStaticHandler)
	}

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
}

func globalHandler(h http.Handler) http.Handler {
	return http.HandlerFunc(func(w http.ResponseWriter, r *http.Request) {

		// set security headers
		headers := w.Header()
		headers.Set("X-Frame-Options", "SAMEORIGIN")
		headers.Set("X-XSS-Protection", "1; mode=block")
		headers.Set("X-Content-Type-Options", "nosniff")
		headers.Set("Strict-Transport-Security", "max-age=31536000; includeSubDomains")
		headers.Set("Referrer-Policy", "same-origin")

		csp := "default-src 'none';"
		csp += "script-src 'self';"
		csp += "connect-src 'self';"
		csp += "img-src 'self';"
		csp += "style-src 'self' 'unsafe-inline';"
		csp += "frame-src 'self' *.youtube.com;"
		headers.Set("Content-Security-Policy", csp)

		user, _ := readEncryptedCookie("user", authSessionExpiry, r)
		var userVal *string
		if user != "" {
			userVal = &user
			setEncryptedCookie("user", user, authSessionExpiry, w)
		}

		userCtx := context.WithValue(r.Context(), authenticatedUser, userVal)
		h.ServeHTTP(w, r.WithContext(userCtx))
	})
}
