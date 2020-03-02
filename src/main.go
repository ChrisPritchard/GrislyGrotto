package main

import (
	"html/template"
	"log"
	"net/http"
	"strconv"

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
		pageParam, hasPage := r.URL.Query()["page"]
		page := 0
		notFirstPage := false
		if hasPage && len(pageParam[0]) > 0 {
			page, _ = strconv.Atoi(pageParam[0])
			if page < 0 {
				page = 0
			}
			if page != 0 {
				notFirstPage = true
			}
		}

		posts, err := getLatestPosts(page)
		if err != nil {
			serverError(w, err)
		}

		model := latestViewModel{notFirstPage, page - 1, page + 1, posts}
		renderView(w, model, views.Latest)
	})

	http.HandleFunc("/post/", func(w http.ResponseWriter, r *http.Request) {
		key := r.URL.Path[len("/post/"):]
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
