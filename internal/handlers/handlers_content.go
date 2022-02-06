package handlers

import (
	"bytes"
	"io"
	"net/http"
	"strings"

	"github.com/ChrisPritchard/GrislyGrotto/internal/config"
	"github.com/ChrisPritchard/GrislyGrotto/pkg/aws"
)

func contentHandler(w http.ResponseWriter, r *http.Request) {
	if r.Method == "GET" {
		tryGetContentFromStorage(w, r)
		return
	} else if r.Method != "POST" {
		http.NotFound(w, r)
		return
	} else if getCurrentUser(r) == nil {
		w.WriteHeader(http.StatusUnauthorized)
		return
	}
	tryUploadContentToStorage(w, r)
}

func tryGetContentFromStorage(w http.ResponseWriter, r *http.Request) {
	filename := r.URL.Path[len("/content/"):]
	if len(filename) == 0 {
		http.NotFound(w, r)
		return
	}

	lowerName := strings.ToLower(filename)
	validExtension := false
	for _, ext := range config.ValidUploadExtensions {
		if strings.HasSuffix(lowerName, ext) {
			validExtension = true
			break
		}
	}
	if !validExtension {
		badRequest(w, r, "bad file extension")
		return
	}

	bytes, exists, err := aws.RetrieveStorageFile(config.ContentStorageName, filename)
	if err != nil {
		serverError(w, r, err)
		return
	}
	if !exists {
		http.NotFound(w, r)
		return
	}

	headers := w.Header()
	contentType, isDownloaded := getContentTypeForExtension(lowerName)
	if isDownloaded {
		headers.Set("Content-Disposition", "attachment; filename=\""+filename+"\"")
	} else {
		headers.Set("Content-Type", contentType)
		headers.Set("X-Content-Type", contentType)
	}
	w.Write(bytes)
}

func getContentTypeForExtension(fileName string) (string, bool) {
	for _, ext := range config.VideoExtensions {
		if strings.HasSuffix(fileName, ext) {
			return "video/mp4", false
		}
	}
	for _, ext := range config.DownloadExtensions {
		if strings.HasSuffix(fileName, ext) {
			return "application/octet-stream", true
		}
	}
	return "image/gif", false
}

func tryUploadContentToStorage(w http.ResponseWriter, r *http.Request) {
	filename := r.URL.Path[len("/content/"):]
	if len(filename) == 0 {
		http.NotFound(w, r)
		return
	}
	filename = strings.ToLower(filename)

	validExtension := false
	for _, ext := range config.ValidUploadExtensions {
		if strings.HasSuffix(filename, ext) {
			validExtension = true
		}
	}
	if !validExtension {
		badRequest(w, r, "bad file extension")
		return
	}

	file, fileHeader, err := r.FormFile("file")
	if err != nil {
		serverError(w, r, err)
		return
	}
	defer file.Close()

	if fileHeader.Size > config.MaxFileSize {
		badRequest(w, r, "file size exceeds maximum")
		return
	}

	buffer := bytes.NewBuffer(nil)
	if _, err := io.Copy(buffer, file); err != nil {
		badRequest(w, r, "Unable to read file")
		return
	}

	mimeType := http.DetectContentType(buffer.Bytes())
	mimeError := checkMimeError(filename, mimeType)
	if mimeError != "" {
		badRequest(w, r, mimeError)
		return
	}

	err = aws.UploadStorageFile(config.ContentStorageName, filename, buffer)
	if err != nil {
		serverError(w, r, err)
		return
	}

	w.WriteHeader(http.StatusAccepted)
}

func checkMimeError(fileName, mimeType string) string {
	for _, ext := range config.VideoExtensions {
		if strings.HasSuffix(fileName, ext) {
			if !strings.HasPrefix(mimeType, "video/") {
				return "File is not a valid video"
			}
			return ""
		}
	}
	for _, ext := range config.DownloadExtensions {
		if strings.HasSuffix(fileName, ext) {
			// note we dont check download extensions, as they could be anything (initially zip)
			return ""
		}
	}
	if !strings.HasPrefix(mimeType, "image/") {
		return "File is not a valid image"
	}
	return ""
}
