package main

import (
	"fmt"
	"html/template"
	"net/http"

	_ "github.com/mattn/go-sqlite3"
)

func main() {

	views := CompileViews()

	http.Handle("/static/",
		http.StripPrefix("/static/",
			http.FileServer(http.Dir("static"))))

	http.HandleFunc("/", func(w http.ResponseWriter, r *http.Request) {
		posts, err := GetLatestPosts()
		if err != nil {
			serverError(w, err)
		}
		model := LatestViewModel{posts}
		renderView(w, model, views.Latest)
	})

	fmt.Println("listening")
	fmt.Println(http.ListenAndServe(":3000", nil))
}

func renderView(w http.ResponseWriter, model interface{}, view *template.Template) {
	if err := view.ExecuteTemplate(w, "master", model); err != nil {
		serverError(w, err)
	}
}

func serverError(w http.ResponseWriter, err error) {
	http.Error(w, err.Error(), http.StatusInternalServerError)
}
