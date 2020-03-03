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
		page, notFirstPage := getPageFromQuery(r)

		posts, err := getLatestPosts(page)
		if err != nil {
			serverError(w, err)
		} else {
			model := latestViewModel{notFirstPage, page - 1, page + 1, posts}
			renderView(w, model, views.Latest)
		}
	})

	http.HandleFunc("/post/", func(w http.ResponseWriter, r *http.Request) {
		key := r.URL.Path[len("/post/"):]
		post, pageNotFound, err := getSinglePost(key)
		if pageNotFound {
			http.NotFound(w, r)
		} else if err != nil {
			serverError(w, err)
		} else {
			renderView(w, post, views.Single)
		}
	})
}

func getPageFromQuery(r *http.Request) (page int, notFirstPage bool) {
	pageParam, hasPage := r.URL.Query()["page"]
	page = 0
	notFirstPage = false
	if hasPage && len(pageParam[0]) > 0 {
		page, _ = strconv.Atoi(pageParam[0])
		if page < 0 {
			page = 0
		}
		if page != 0 {
			notFirstPage = true
		}
	}
	return page, notFirstPage
}

func renderView(w http.ResponseWriter, model interface{}, view *template.Template) {
	if err := view.ExecuteTemplate(w, "master", model); err != nil {
		serverError(w, err)
	}
}

func serverError(w http.ResponseWriter, err error) {
	http.Error(w, err.Error(), http.StatusInternalServerError)
}
