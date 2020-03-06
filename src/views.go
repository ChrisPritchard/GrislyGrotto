package main

import (
	"html/template"
	"net/http"
	"time"
)

type views struct {
	Latest, Single, Archives, Month, Search, About, Login *template.Template
}

func compileViews() views {
	return views{
		Latest:   createView("latest.html"),
		Single:   createView("single.html"),
		Archives: createView("archives.html"),
		Month:    createView("month.html"),
		Search:   createView("search.html"),
		About:    createView("about.html"),
		Login:    createView("login.html"),
	}
}

func createView(contentFileName string) *template.Template {
	baseDir := "templates/"
	funcMap := template.FuncMap{
		"raw":        raw,
		"formatDate": formatDate,
		"loggedIn":   func() bool { return false }, // overriden on renderView
	}
	return template.Must(template.New("").Funcs(funcMap).ParseFiles(baseDir+contentFileName, baseDir+"_master.html"))
}

func raw(s string) template.HTML {
	return template.HTML(s)
}

func formatDate(s string) string {
	asTime, _ := time.Parse("2006-01-02 15:04:05", s)
	return asTime.Format("15:04 PM, on Monday, 02 January 2006")
}

func renderView(w http.ResponseWriter, r *http.Request, model interface{}, view *template.Template) {
	_, err := readCookie("user", r)
	loggedIn := err == nil
	view.Funcs(template.FuncMap{"loggedIn": func() bool { return loggedIn }})

	if err := view.ExecuteTemplate(w, "master", model); err != nil {
		serverError(w, err)
	}
}
