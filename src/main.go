package main

import (
	"context"
	"crypto/rand"
	"database/sql"
	"encoding/base64"
	"flag"
	"fmt"
	"io/ioutil"
	"log"
	"net/http"
	"os"
	"strings"
	"time"

	_ "github.com/mattn/go-sqlite3"
)

func main() {
	log.SetFlags(0)
	log.SetOutput(os.Stdout)

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
		csp += "style-src 'self' 'unsafe-inline';"
		csp += "frame-src 'self' *.youtube.com;"
		headers.Set("Content-Security-Policy", csp)

		user, _ := readEncryptedCookie("user", r)
		var userVal *string
		if user != "" {
			userVal = &user
		}

		userCtx := context.WithValue(r.Context(), authenticatedUser, userVal)
		h.ServeHTTP(w, r.WithContext(userCtx))

		if user != "" {
			setEncryptedCookie("user", user, w)
		}
	})
}

func getConfig() bool {
	connectionString = defaultConnectionString
	listenURL = defaultListenAddr

	connArg := flag.String("db", "", "the sqlite connection string\n\tdefaults to "+defaultConnectionString)
	urlArg := flag.String("url", "", "the url with port to listen to\n\tdefaults to "+defaultListenAddr)
	envArg := flag.Bool("dev", false, "sets to run in 'dev' mode\n\tif set resources are loaded on request rather than embedded")
	embedArg := flag.Bool("embed", false, "base64 encodes all static resources into a embedded.go file, then exits")
	setAuthorArg := flag.Bool("setauthor", false, "creates or updates a login account, then exits\nshould be followed by [username] [password] [displayname]")
	flag.Parse()

	if *embedArg {
		embedAssets()
		log.Println("assets embedded successfully")
		return false
	}

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

	isDevelopment = *envArg
	return true
}

func embedAssets() {
	paths := []string{"./static", "./templates"}

	out, err := os.Create("embedded.go")
	defer out.Close()

	if err != nil {
		log.Fatal(err)
	}

	header := fmt.Sprintf(`
// generated by the main.go embedAssets func
// DO NOT EDIT
// %s
package main
var embeddedAssets = map[string]string {
`, time.Now().Format(time.RFC3339))
	fmt.Fprint(out, header)

	for _, path := range paths {
		files, err := ioutil.ReadDir(path)
		if err != nil {
			log.Fatal(err)
		}

		for _, file := range files {
			name := file.Name()
			file, _ := ioutil.ReadFile(path + "/" + name)
			content := base64.StdEncoding.EncodeToString(file)
			fmt.Fprintf(out, "\t\"%s/%s\": \"%s\",\n", path, name, content)
		}
	}

	fmt.Fprint(out, "\n}\n")
}
