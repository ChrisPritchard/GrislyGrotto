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

	posts := make(chan data.BlogPost)
	//errors := make(chan error)
	go data.GetAllPostsAsync(posts) //, errors)

	headers := w.Header()
	headers.Set("Content-Disposition", fmt.Sprintf("attachment; filename=posts-%s.zip", time.Now().Format("2006-01-02")))
	headers.Set("Content-Type", "application/zip")
	headers.Set("X-Content-Type", "application/zip")

	zipWriter := zip.NewWriter(w)

	for p := range posts {
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
