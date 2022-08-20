package handlers

import (
	"archive/zip"
	"bytes"
	"encoding/json"
	"fmt"
	"net/http"
	"time"

	"github.com/ChrisPritchard/GrislyGrotto/internal/config"
	"github.com/ChrisPritchard/GrislyGrotto/internal/data"
	"github.com/ChrisPritchard/GrislyGrotto/pkg/aws"
)

func validateBasicAuth(w http.ResponseWriter, r *http.Request) bool {
	username, password, ok := r.BasicAuth()
	if !ok {
		w.Header().Set("WWW-Authenticate", "Basic realm=\"grislygrotto.nz\"")
		unauthorised(w)
		return false
	}
	valid, errorMessage := validateCredentials(r, username, password)
	if !valid {
		w.Header().Set("WWW-Authenticate", "Basic realm=\"grislygrotto.nz\"")
		http.Error(w, errorMessage, http.StatusUnauthorized)
		return false
	}
	return true
}

func postsBackupHandler(w http.ResponseWriter, r *http.Request) {
	if r.Method != "GET" {
		http.NotFound(w, r)
		return
	}

	if !validateBasicAuth(w, r) {
		return
	}

	posts := make(chan data.StreamedBlogPost)
	go data.GetAllPostsAsync(posts)

	buffer := bytes.NewBuffer(nil)
	zipWriter := zip.NewWriter(buffer)
	defer zipWriter.Close()

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

	filename := fmt.Sprintf("posts-%s.zip", time.Now().Format("2006-01-02"))
	aws.UploadStorageFile(config.ContentStorageName, filename, buffer)
	link, err := aws.CreateTempLink(config.ContentStorageName, filename)
	if err != nil {
		serverError(w, r, err)
		return
	}
	http.Redirect(w, r, link, http.StatusFound)
}

func contentBackupHandler(w http.ResponseWriter, r *http.Request) {
	if r.Method != "GET" {
		http.NotFound(w, r)
		return
	}

	if !validateBasicAuth(w, r) {
		return
	}

	files, err := aws.AllFiles(config.ContentStorageName)
	if err != nil {
		serverError(w, r, err)
		return
	}

	buffer := bytes.NewBuffer(nil)
	zipWriter := zip.NewWriter(buffer)
	defer zipWriter.Close()

	for _, fileName := range files {
		data, _, err := aws.RetrieveStorageFile(config.ContentStorageName, fileName)
		if err != nil {
			serverError(w, r, err)
			return
		}

		header := zip.FileHeader{Name: fileName, Method: zip.Deflate}
		writer, err := zipWriter.CreateHeader(&header)
		if err != nil {
			serverError(w, r, err)
			return
		}

		writer.Write(data)
	}

	filename := fmt.Sprintf("content-%s.zip", time.Now().Format("2006-01-02"))
	aws.UploadStorageFile(config.ContentStorageName, filename, buffer)
	link, err := aws.CreateTempLink(config.ContentStorageName, filename)
	if err != nil {
		serverError(w, r, err)
		return
	}
	http.Redirect(w, r, link, http.StatusFound)
}
