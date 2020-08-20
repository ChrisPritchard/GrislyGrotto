package main

import (
	"net/http"

	"github.com/aws/aws-sdk-go/aws"
	"github.com/aws/aws-sdk-go/aws/session"
	"github.com/aws/aws-sdk-go/service/s3"
	"github.com/aws/aws-sdk-go/service/s3/s3manager"
)

func tryGetContentFromStorage(w http.ResponseWriter, r *http.Request) {
	filename := r.URL.Path[len("/content/"):]
	if len(filename) == 0 {
		http.NotFound(w, r)
		return
	}

	session, err := session.NewSessionWithOptions(session.Options{
		SharedConfigState: session.SharedConfigEnable,
	})
	if err != nil {
		serverError(w, err)
	}

	downloader := s3manager.NewDownloader(session)
	buf := aws.NewWriteAtBuffer([]byte{})
	_, err = downloader.Download(buf, &s3.GetObjectInput{
		Bucket: aws.String(contentStorageName),
		Key:    aws.String(filename),
	})

	if err != nil {
		serverError(w, err)
	}

	setMimeType(w, r)
	w.Write(buf.Bytes())
}

func tryUploadContentToStorage(w http.ResponseWriter, r *http.Request) {
	filename := r.URL.Path[len("/content/"):]
	if len(filename) == 0 {
		http.NotFound(w, r)
		return
	}

	file, _, err := r.FormFile("file")
	if err != nil {
		serverError(w, err)
	}
	defer file.Close()

	session, err := session.NewSessionWithOptions(session.Options{
		SharedConfigState: session.SharedConfigEnable,
	})
	if err != nil {
		serverError(w, err)
	}

	uploader := s3manager.NewUploader(session)
	_, err = uploader.Upload(&s3manager.UploadInput{
		Bucket: aws.String(contentStorageName),
		Key:    aws.String(filename),
		Body:   file,
	})

	if err != nil {
		serverError(w, err)
	}

	w.WriteHeader(http.StatusAccepted)
}
