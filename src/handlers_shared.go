package main

import (
	"bytes"
	"encoding/base64"
	"html/template"
	"net/http"
	"path/filepath"
	"strings"
	"time"
)

func ipOnly(ipAndPort string) string {
	portMarker := strings.LastIndex(ipAndPort, ":")
	if portMarker == -1 {
		return ipAndPort
	}
	return ipAndPort[:portMarker]
}

func getIP(r *http.Request) string {
	forwarded := r.Header.Get("x-forwarded-for") // case is normalised
	if forwarded == "" {
		return ipOnly(r.RemoteAddr)
	}
	return ipOnly(strings.Split(forwarded, ", ")[0])
}

func setBlockTime(r *http.Request, username string) {
	blocked[getIP(r)] = time.Now().Unix()
	if username != "" {
		blocked[username] = time.Now().Unix()
	}
}

func cleanBlocked() {
	now := time.Now().Unix()
	for k, v := range blocked {
		if now-v > blockTime {
			delete(blocked, k)
		}
	}
}

func getBlockTime(r *http.Request, username string) int {
	now := time.Now().Unix()
	time1, time2 := now-blocked[getIP(r)], now-blocked[username]
	if time1 > blockTime && time2 > blockTime {
		return 0
	}
	cleanBlocked() // done on blocking to not affect ligitimate users (except commenters)
	if time2 < time1 {
		return blockTime - int(time2)
	}
	return blockTime - int(time1)
}

func embeddedStaticHandler(w http.ResponseWriter, r *http.Request) {
	file := r.URL.Path[len("/static/"):]
	ext := filepath.Ext(file)

	var fileContent string
	if content, exists := embeddedStatics[file]; exists {
		fileContent = content
	} else {
		http.NotFound(w, r)
		return
	}

	setMimeType(w, r)

	if ext == ".png" {
		bytes := make([]byte, base64.StdEncoding.DecodedLen(len(fileContent)))
		base64.StdEncoding.Decode(bytes, []byte(fileContent))
		w.Write(bytes)
	} else {
		w.Write([]byte(fileContent))
	}
}

func runtimeStaticHandler() http.Handler {
	server := http.FileServer(http.Dir("static"))
	fileHandler := http.HandlerFunc(func(w http.ResponseWriter, r *http.Request) {
		setMimeType(w, r)
		server.ServeHTTP(w, r)
	})

	return http.StripPrefix("/static/", fileHandler)
}

func setMimeType(w http.ResponseWriter, r *http.Request) {
	headers := w.Header()
	ext := filepath.Ext(r.URL.Path)

	switch ext {
	case ".css":
		headers.Set("Content-Type", "text/css")
	case ".js":
		headers.Set("Content-Type", "application/javascript")
	case ".png":
		headers.Set("Content-Type", "image/png")
	default:
		return
	}
}

func themeHandler(w http.ResponseWriter, r *http.Request) {
	if r.Method != "POST" {
		http.NotFound(w, r)
		return
	}
	err := r.ParseForm()
	if err != nil {
		serverError(w, err)
	}
	setCookie("theme", r.Form["current-theme"][0], time.Now().Add(themeExpiry), w)
	http.Redirect(w, r, "/"+r.Form["return-path"][0], http.StatusFound)
}

func serverError(w http.ResponseWriter, err error) {
	http.Error(w, err.Error(), http.StatusInternalServerError)
}

func badRequest(w http.ResponseWriter, message string) {
	http.Error(w, message, http.StatusBadRequest)
}

func unauthorised(w http.ResponseWriter) {
	http.Error(w, "unauthorised", http.StatusUnauthorized)
}

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
	var buf bytes.Buffer
	source := []byte(s)
	if err := markdownEngine.Convert(source, &buf); err != nil {
		return template.HTML("<b>Error parsing Markdown, falling back to raw</b><br/>" + s)
	}

	return template.HTML(buf.String())
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
