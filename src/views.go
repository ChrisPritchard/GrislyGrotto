package main

import "html/template"

type views struct {
	Latest, Single *template.Template
}

func compileViews() views {
	return views{
		Latest: createView("latest.html"),
		Single: createView("single.html"),
	}
}

func createView(contentFileName string) *template.Template {
	baseDir := "templates/"
	funcMap := template.FuncMap{"raw": raw}
	return template.Must(template.New("").Funcs(funcMap).ParseFiles(baseDir+contentFileName, baseDir+"_master.html"))
}

func raw(s string) template.HTML {
	return template.HTML(s)
}
