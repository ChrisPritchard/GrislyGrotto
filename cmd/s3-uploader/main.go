package main

import (
	"bytes"
	"fmt"
	"io"
	"log"
	"net/http"
	"os"
	"strings"

	"github.com/ChrisPritchard/GrislyGrotto/pkg/aws"
	"github.com/ChrisPritchard/GrislyGrotto/pkg/lambda"
)

const (
	authAccessKey = "ACCESSKEY"
	bucketNameKey = "S3BUCKET"
	listFileHTML  = "<li><a href='?ACCESSKEY=%s&filename=%s'>%s</a></li>"
	uploadHTML    = "<!doctype html><html><head><meta charset='utf-8'></head></body><div>[FILES]</div><form method='POST' enctype='multipart/form-data'><input type='file' name='file' /><input type='submit' value='submit' /></form></body></html>"
)

func main() {
	log.SetFlags(0)
	log.SetOutput(os.Stdout)

	lambda.Start(http.HandlerFunc(contentHandler))
}

func contentHandler(w http.ResponseWriter, r *http.Request) {
	if r.URL.Query().Get(authAccessKey) != os.Getenv(authAccessKey) || r.FormValue(authAccessKey) != os.Getenv(authAccessKey) {
		http.Error(w, "missing or invalid credentials", http.StatusUnauthorized)
		return
	}

	if r.Method == "GET" {
		fileName := r.URL.Query().Get("filename")
		if fileName != "" {
			retrieveFile(w, r, fileName)
			return
		}
		showFormWithList(w, r)
		return
	}

	if r.Method != "POST" {
		http.NotFound(w, r)
		return
	}

	file, fileInfo, err := r.FormFile("file")
	if err != nil {
		log.Println(err.Error())
		http.Error(w, "couldn't read file from form with error: "+err.Error(), http.StatusBadRequest)
		return
	}
	defer file.Close()

	buffer := bytes.NewBuffer(nil)
	if _, err := io.Copy(buffer, file); err != nil {
		log.Println(err.Error())
		http.Error(w, "failed to copy file to buffer with error:"+err.Error(), http.StatusBadRequest)
		return
	}

	if err := aws.UploadStorageFile(os.Getenv(bucketNameKey), fileInfo.Filename, buffer); err != nil {
		log.Println(err.Error())
		http.Error(w, "failed to write file to EFS with error: "+err.Error(), http.StatusBadRequest)
		return
	}

	http.Redirect(w, r, "/?ACCESSKEY="+os.Getenv(authAccessKey)+"&success=true", http.StatusFound)
}

func retrieveFile(w http.ResponseWriter, r *http.Request, fileName string) {
	link, err := aws.CreateTempLink(os.Getenv(bucketNameKey), fileName)
	if err != nil {
		log.Println(err.Error())
		http.Error(w, "failed to retrieve tempt link for file with error: "+err.Error(), http.StatusBadRequest)
		return
	}
	http.Redirect(w, r, link, http.StatusFound)
}

func showFormWithList(w http.ResponseWriter, r *http.Request) {
	files, err := aws.AllFiles(os.Getenv(bucketNameKey))
	if err != nil {
		log.Println(err.Error())
		http.Error(w, "failed to read directory on efs with error: "+err.Error(), http.StatusBadRequest)
		return
	}

	fileText := "<ul>"
	if r.URL.Query().Get("success") == "true" {
		fileText = "<div>file uploaded successfully</div>" + fileText
	}

	for _, v := range files {
		fileText += fmt.Sprintf(listFileHTML, os.Getenv(authAccessKey), v, v)
	}
	fileText += "</ul>"

	w.Header().Set("Content-Type", "text/html")
	w.Write([]byte(strings.Replace(uploadHTML, "[FILES]", fileText, 1)))
}
