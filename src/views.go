package main

import (
	"html/template"
	"time"
)

type views struct {
	Latest, Single, Search, Archives *template.Template
}

func compileViews() views {
	return views{
		Latest:   createView("latest.html"),
		Single:   createView("single.html"),
		Search:   createView("search.html"),
		Archives: createView("archives.html"),
	}
}

func createView(contentFileName string) *template.Template {
	baseDir := "templates/"
	funcMap := template.FuncMap{"raw": raw, "formatDate": formatDate}
	return template.Must(template.New("").Funcs(funcMap).ParseFiles(baseDir+contentFileName, baseDir+"_master.html"))
}

func raw(s string) template.HTML {
	return template.HTML(s)
}

func formatDate(s string) string {
	asTime, _ := time.Parse("2006-01-02 15:04:05", s)
	return asTime.Format("15:04 PM, on Monday, 02 January 2006")
}
