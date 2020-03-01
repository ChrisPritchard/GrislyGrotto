package main

import (
	"fmt"
	"html/template"
	"net/http"
	"time"
)

type HomeModel struct {
	Posts []BlogPost
}

type BlogPost struct {
	Title, Body string
	Date        time.Time
}

func main() {

	templates := template.Must(template.ParseFiles("templates/home.html"))

	homeModel := HomeModel{
		[]BlogPost{
			BlogPost{"test1", "Loren Ipsum Dolor Sit Amet", time.Now()},
			BlogPost{"test2", "Loren Ipsum Dolor Sit Amet", time.Now()},
			BlogPost{"test3", "Loren Ipsum Dolor Sit Amet", time.Now()},
		},
	}

	http.Handle("/static/",
		http.StripPrefix("/static/",
			http.FileServer(http.Dir("static"))))

	http.HandleFunc("/", func(w http.ResponseWriter, r *http.Request) {
		if err := templates.ExecuteTemplate(w, "home.html", homeModel); err != nil {
			http.Error(w, err.Error(), http.StatusInternalServerError)
		}
	})

	fmt.Println("listening")
	fmt.Println(http.ListenAndServe(":8080", nil))
}
