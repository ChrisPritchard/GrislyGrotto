package main

import (
	"html/template"
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

	http.HandleFunc("/", func(w http.ResponseWriter, r *http.Request) {
		posts, err := getLatestPosts()
		if err != nil {
			serverError(w, err)
		}
		model := latestViewModel{posts}
		renderView(w, model, views.Latest)
	})

	http.HandleFunc("/post/", func(w http.ResponseWriter, r *http.Request) {
		key := r.URL.Path["/post/":]
		post, err := getSinglePost(key)
		if err != nil {
			serverError(w, err)
		}
		renderView(w, post, views.Single)
	})

	// /			latest
	//	-> next page
	//	-> previous page
	// /post/key	single
	//	-> post comment
	//	-> delete comment (if author)
	// /login		login
	//	-> try login
	// /new			new post
	//	-> try create
	// /edit/key	edit
	//	-> try edit
	// /archives
	// /month
	// /search
	//	-> get results
	// /about
}

func renderView(w http.ResponseWriter, model interface{}, view *template.Template) {
	if err := view.ExecuteTemplate(w, "master", model); err != nil {
		serverError(w, err)
	}
}

func serverError(w http.ResponseWriter, err error) {
	http.Error(w, err.Error(), http.StatusInternalServerError)
}
