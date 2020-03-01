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

type HomeModel struct {
	Posts []BlogPost
}

type BlogPost struct {
	Title, Body string
	Date        time.Time
}

func main() {

	homeTemplate := template.Must(template.New("").Funcs(template.FuncMap{"unescape": unescape}).ParseFiles("templates/home.html", "templates/_master.html"))

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

	homeModel := HomeModel{
		posts,
	}

	http.Handle("/static/",
		http.StripPrefix("/static/",
			http.FileServer(http.Dir("static"))))

	http.HandleFunc("/", func(w http.ResponseWriter, r *http.Request) {

		if err := homeTemplate.ExecuteTemplate(w, "master", homeModel); err != nil {
			http.Error(w, err.Error(), http.StatusInternalServerError)
		}
	})

	fmt.Println("listening")
	fmt.Println(http.ListenAndServe(":3000", nil))
}

func unescape(s string) template.HTML {
	return template.HTML(s)
}