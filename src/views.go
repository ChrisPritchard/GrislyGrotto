package main

import "html/template"

// Views is a set of templates compiled for the website
type Views struct {
	Latest *template.Template
}

func raw(s string) template.HTML {
	return template.HTML(s)
}

func createView(contentFileName string) *template.Template {
	baseDir := "templates/"
	funcMap := template.FuncMap{"raw": raw}
	return template.Must(template.New("").Funcs(funcMap).ParseFiles(baseDir+contentFileName, baseDir+"_master.html"))
}

// CompileViews will create and return a set of all templates in the application
func CompileViews() Views {
	return Views{
		Latest: createView("home.html"),
	}
}
