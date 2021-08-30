package handlers

import (
	"archive/zip"
	"encoding/json"
	"fmt"
	"net/http"
	"time"

	"github.com/ChrisPritchard/GrislyGrotto/internal/data"
)

func postsBackupHandler(w http.ResponseWriter, r *http.Request) {
	if r.Method != "GET" {
		http.NotFound(w, r)
		return
	}

	username, password, ok := r.BasicAuth()
	if !ok {
		w.Header().Set("WWW-Authenticate", "Basic realm=\"grislygrotto.nz\"")
		unauthorised(w)
		return
	}
	valid, errorMessage := validateCredentials(r, username, password)
	if !valid {
		w.Header().Set("WWW-Authenticate", "Basic realm=\"grislygrotto.nz\"")
		http.Error(w, errorMessage, http.StatusUnauthorized)
		return
	}

	posts := make(chan data.StreamedBlogPost)
	go data.GetAllPostsAsync(posts)

	headers := w.Header()
	headers.Set("Content-Disposition", fmt.Sprintf("attachment; filename=posts-%s.zip", time.Now().Format("2006-01-02")))
	headers.Set("Content-Type", "application/zip")
	headers.Set("X-Content-Type", "application/zip")

	zipWriter := zip.NewWriter(w)

	for message := range posts {
		if message.Error != nil {
			serverError(w, r, message.Error)
			return
		}
		p := message.Post

		date, _ := time.Parse("2006-01-02 15:04:05", p.Date)
		header := zip.FileHeader{Name: fmt.Sprintf("%s-%s.json", date.Format("2006-01-02-15-04-05-PM"), p.Key), Method: zip.Deflate}
		writer, err := zipWriter.CreateHeader(&header)
		if err != nil {
			serverError(w, r, err)
			return
		}

		jsonEncoder := json.NewEncoder(writer)
		err = jsonEncoder.Encode(p)
		if err != nil {
			serverError(w, r, err)
			return
		}
	}

	defer zipWriter.Close()
}
