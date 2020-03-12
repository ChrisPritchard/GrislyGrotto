package main

import (
	"html/template"
	"net/http"
	"time"
)

type views struct {
	Latest, Single, Archives, Month, Search, About, Login, Editor *template.Template
}

// this gets populated via go generate in main
var templateContent map[string]string

func compileViews() {
	templateContent = make(map[string]string)
	loadTemplates() // function in the generated file

	compiledViews = views{
		Latest:   createView("latest.html"),
		Single:   createView("single.html"),
		Archives: createView("archives.html"),
		Month:    createView("month.html"),
		Search:   createView("search.html"),
		About:    createView("about.html"),
		Login:    createView("login.html"),
		Editor:   createView("editor.html"),
	}
}

func createView(contentFileName string) *template.Template {
	funcMap := template.FuncMap{
		"raw":        raw,
		"formatDate": formatDate,
		"loggedIn":   func() bool { return false }, // overriden on renderView
	}
	baseTemplate := template.New("").Funcs(funcMap)
	result, err := baseTemplate.Parse(templateContent[contentFileName])
	return template.Must(result, err)
}

func raw(s string) template.HTML {
	return template.HTML(s)
}

func render(s string) template.HTML {
	if s[:len(markdownToken)] == markdownToken {
		// convert to markdown
		return raw(s)
	}
	return raw(s)
}

func formatDate(s string) string {
	asTime, _ := time.Parse("2006-01-02 15:04:05", s)
	return asTime.Format("15:04 PM, on Monday, 02 January 2006")
}

func renderView(w http.ResponseWriter, r *http.Request, model interface{}, view *template.Template) {
	loggedIn := currentUser != ""

	view.Funcs(template.FuncMap{"loggedIn": func() bool { return loggedIn }})
	if err := view.ExecuteTemplate(w, "master", model); err != nil {
		serverError(w, err)
	}
}
