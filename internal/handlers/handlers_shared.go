package handlers

import (
	"bytes"
	"html/template"
	"net/http"
	"path/filepath"
	"strings"
	"time"

	"github.com/ChrisPritchard/GrislyGrotto/internal/config"
	"github.com/ChrisPritchard/GrislyGrotto/internal/cookies"
	"github.com/ChrisPritchard/GrislyGrotto/internal/embedded"
)

func getCurrentUser(r *http.Request) *string {
	return r.Context().Value(config.AuthenticatedUser).(*string)
}

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
	config.Blocked[getIP(r)] = time.Now().Unix()
	if username != "" {
		config.Blocked[username] = time.Now().Unix()
	}
}

func cleanBlocked() {
	now := time.Now().Unix()
	for k, v := range config.Blocked {
		if now-v > config.BlockTime {
			delete(config.Blocked, k)
		}
	}
}

func getBlockTime(r *http.Request, username string) int {
	now := time.Now().Unix()
	time1, time2 := now-config.Blocked[getIP(r)], now-config.Blocked[username]
	if time1 > config.BlockTime && time2 > config.BlockTime {
		return 0
	}
	cleanBlocked() // done on blocking to not affect ligitimate users (except commenters)
	if time2 < time1 {
		return config.BlockTime - int(time2)
	}
	return config.BlockTime - int(time1)
}

func embeddedStaticHandler(w http.ResponseWriter, r *http.Request) {
	file := r.URL.Path[len("/static/"):]

	if content, err := embedded.Resources.ReadFile("static/" + file); err == nil {
		setMimeType(w, r)
		w.Write(content)
		return
	}

	http.NotFound(w, r)
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
		headers.Set("X-Content-Type", "image/png")
	case ".gif":
		headers.Set("Content-Type", "image/gif")
		headers.Set("X-Content-Type", "image/gif")
	case ".jpg":
		headers.Set("Content-Type", "image/jpeg")
		headers.Set("X-Content-Type", "image/jpeg")
	case ".jpeg":
		headers.Set("Content-Type", "image/jpeg")
		headers.Set("X-Content-Type", "image/jpeg")
	default:
		headers.Set("Content-Type", "text/plain")
	}
}

func themeHandler(w http.ResponseWriter, r *http.Request) {
	if r.Method != "POST" {
		http.NotFound(w, r)
		return
	}
	cookies.SetCookie("theme", r.FormValue("current-theme"), time.Now().Add(config.ThemeExpiry), w)
	http.Redirect(w, r, "/"+r.FormValue("return-path"), http.StatusFound)
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
	loggedIn := getCurrentUser(r) != nil

	tmpl := template.New("").Funcs(template.FuncMap{
		"raw":           raw,
		"renderPost":    renderPost,
		"renderComment": renderComment,
		"formatDate":    formatDate,
		"loggedIn":      func() bool { return loggedIn },
		"page":          func() string { return pageTitle },
		"path":          func() string { return r.URL.Path[1:] },
		"theme":         func() string { return getTheme(r) },
		"isNewPost":     func() bool { return pageTitle == "New Post" }})

	masterContent, _ := embedded.Resources.ReadFile("templates/master.html")
	templateContent, _ := embedded.Resources.ReadFile("templates/" + templateFile)
	result, err := tmpl.Parse(string(masterContent) + string(templateContent))
	tmpl = template.Must(result, err)

	if err := tmpl.ExecuteTemplate(w, "master", model); err != nil {
		serverError(w, err)
	}
}

func raw(s string) template.HTML {
	return template.HTML(s)
}

func renderPost(s string) template.HTML {
	var buf bytes.Buffer
	source := []byte(s)
	if err := config.MarkdownFull.Convert(source, &buf); err != nil {
		return template.HTML("<b>Error parsing Markdown, falling back to raw</b><br/>" + s)
	}

	return template.HTML(buf.String())
}

func renderComment(s string) template.HTML {
	var buf bytes.Buffer
	source := []byte(s)
	if err := config.MarkdownRestricted.Convert(source, &buf); err != nil {
		return template.HTML("<span class='error'>Failed to render comment</span>")
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
