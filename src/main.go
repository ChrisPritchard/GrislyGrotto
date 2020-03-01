package main

import (
	"database/sql"
	"fmt"
	"html/template"
	"log"
	"net/http"
	"time"

	_ "github.com/mattn/go-sqlite3"
)

func main() {

	views := CompileViews()

	database, err := sql.Open("sqlite3", "./grislygrotto.db")
	if err != nil {
		log.Fatal(err)
	}

	posts := make([]BlogPost, 0)
	rows, err := database.Query("SELECT Title, Content, Date FROM Posts ORDER BY Date DESC LIMIT 5")
	var title, content string
	var date time.Time
	for rows.Next() {
		rows.Scan(&title, &content, &date)
		posts = append(posts, BlogPost{title, content, date})
	}

	model := LatestViewModel{
		posts,
	}

	http.Handle("/static/",
		http.StripPrefix("/static/",
			http.FileServer(http.Dir("static"))))

	http.HandleFunc("/", func(w http.ResponseWriter, r *http.Request) {
		renderView(w, model, views.Latest)
	})

	fmt.Println("listening")
	fmt.Println(http.ListenAndServe(":3000", nil))
}

func renderView(w http.ResponseWriter, model interface{}, view *template.Template) {
	if err := view.ExecuteTemplate(w, "master", model); err != nil {
		http.Error(w, err.Error(), http.StatusInternalServerError)
	}
}
