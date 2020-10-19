package main

import (
	"mime/multipart"
	"net/http"
	"strings"

	"github.com/aws/aws-sdk-go/aws"
	"github.com/aws/aws-sdk-go/aws/session"
	"github.com/aws/aws-sdk-go/service/s3"
	"github.com/aws/aws-sdk-go/service/s3/s3manager"
)

func retrieveStorageFile(filename string) (bytes []byte, exists bool, err error) {
	session, err := session.NewSessionWithOptions(session.Options{
		SharedConfigState: session.SharedConfigEnable,
	})
	if err != nil {
		return nil, false, err
	}

	downloader := s3manager.NewDownloader(session)
	buf := aws.NewWriteAtBuffer([]byte{})
	_, err = downloader.Download(buf, &s3.GetObjectInput{
		Bucket: aws.String(contentStorageName),
		Key:    aws.String(filename),
	})

	if err != nil {
		return nil, false, err
	}

	return buf.Bytes(), true, nil
}

func tryGetContentFromStorage(w http.ResponseWriter, r *http.Request) {
	filename := r.URL.Path[len("/content/"):]
	if len(filename) == 0 {
		http.NotFound(w, r)
		return
	}

	lowerName := strings.ToLower(filename)
	validExtension := false
	for _, ext := range validUploadExtensions {
		if strings.HasSuffix(lowerName, ext) {
			validExtension = true
		}
	}
	if !validExtension {
		badRequest(w, "bad file extension")
		return
	}

	bytes, exists, err := retrieveStorageFile(filename)
	if err != nil {
		serverError(w, err)
		return
	}
	if !exists {
		http.NotFound(w, r)
		return
	}

	headers := w.Header()
	headers.Set("Content-Type", "image/gif")
	headers.Set("X-Content-Type", "image/gif")
	w.Write(bytes)
}

func uploadStorageFile(filename string, file multipart.File) error {
	session, err := session.NewSessionWithOptions(session.Options{
		SharedConfigState: session.SharedConfigEnable,
	})
	if err != nil {
		return err
	}

	uploader := s3manager.NewUploader(session)
	_, err = uploader.Upload(&s3manager.UploadInput{
		Bucket: aws.String(contentStorageName),
		Key:    aws.String(filename),
		Body:   file,
	})

	return err
}

func tryUploadContentToStorage(w http.ResponseWriter, r *http.Request) {
	filename := r.URL.Path[len("/content/"):]
	if len(filename) == 0 {
		http.NotFound(w, r)
		return
	}
	filename = strings.ToLower(filename)

	validExtension := false
	for _, ext := range validUploadExtensions {
		if strings.HasSuffix(filename, ext) {
			validExtension = true
		}
	}
	if !validExtension {
		badRequest(w, "bad file extension")
		return
	}

	file, fileHeader, err := r.FormFile("file")
	if err != nil {
		serverError(w, err)
		return
	}
	defer file.Close()

	if fileHeader.Size > maxFileSize {
		badRequest(w, "file size exceeds maximum")
		return
	}

	buffer := make([]byte, 512)
	if _, err = file.Read(buffer); err != nil {
		serverError(w, err)
		return
	}

	mimeType := http.DetectContentType(buffer)
	if !strings.HasPrefix(mimeType, "image/") {
		badRequest(w, "file is not a valid image")
		return
	}

	err = uploadStorageFile(filename, file)
	if err != nil {
		serverError(w, err)
		return
	}

	w.WriteHeader(http.StatusAccepted)
}
