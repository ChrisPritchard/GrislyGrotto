package main

import (
	"bytes"
	"html/template"
	"net/http"
	"time"

	"github.com/yuin/goldmark"
)

func renderView(w http.ResponseWriter, r *http.Request, model interface{}, templateFile, pageTitle string) {
	loggedIn := currentUser != ""

	tmpl := template.New("").Funcs(template.FuncMap{
		"raw":        raw,
		"render":     render,
		"formatDate": formatDate,
		"loggedIn":   func() bool { return loggedIn },
		"page":       func() string { return pageTitle },
		"path":       func() string { return r.URL.Path[1:] },
		"theme":      func() string { return getTheme(r) },
		"isNewPost":  func() bool { return pageTitle == "New Post" }})

	if isDevelopment {
		result, err := tmpl.ParseFiles("templates/_master.html", "templates/"+templateFile)
		tmpl = template.Must(result, err)
	} else {
		result, err := tmpl.Parse(templateContent[templateFile])
		tmpl = template.Must(result, err)
	}

	if err := tmpl.ExecuteTemplate(w, "master", model); err != nil {
		serverError(w, err)
	}
}

func raw(s string) template.HTML {
	return template.HTML(s)
}

func render(s string) template.HTML {
	if s[:len(markdownToken)] != markdownToken {
		return raw(s)
	}

	var buf bytes.Buffer
	source := []byte(s[len(markdownToken):])
	if err := goldmark.Convert(source, &buf); err != nil {
		return raw("<b>Error parsing Markdown, falling back to raw</b><br/>" + s)
	}

	return raw(buf.String())
}

func formatDate(s string) string {
	asTime, _ := time.Parse("2006-01-02 15:04:05", s)
	return asTime.Format("15:04 PM, on Monday, 02 January 2006")
}

func getTheme(r *http.Request) string {
	currentTheme := "green-black"
	cookie, err := r.Cookie("theme")
	if err != nil {
		return currentTheme
	}
	return cookie.Value
}
