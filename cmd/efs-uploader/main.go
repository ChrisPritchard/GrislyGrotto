package main

import (
	"bytes"
	"fmt"
	"io"
	"io/ioutil"
	"log"
	"net/http"
	"os"
	"strings"

	"github.com/ChrisPritchard/GrislyGrotto/pkg/lambda"
)

const (
	authAccessKey  = "ACCESSKEY"
	defaultPathEnv = "UPLOADPATH"
	uploadHTML     = "<html><div>[FILES]</div><form method='POST' enctype='multipart/form-data'><input type='file' name='file' /><input type='submit' value='submit' /></form></html>"
)

func main() {
	log.SetFlags(0)
	log.SetOutput(os.Stdout)

	lambda.Start(http.HandlerFunc(contentHandler))
}

func contentHandler(w http.ResponseWriter, r *http.Request) {
	if r.URL.Query().Get(authAccessKey) != os.Getenv(authAccessKey) {
		http.Error(w, "missing or invalid credentials", http.StatusUnauthorized)
		return
	}

	folder := os.Getenv(defaultPathEnv)
	if !strings.HasSuffix(folder, "/") {
		folder += "/"
	}

	if r.Method == "GET" {
		files, _ := ioutil.ReadDir(folder)
		fileText := "<ul>"
		for _, v := range files {
			fileText += fmt.Sprintf("<li>%s</li>", v.Name())
		}
		fileText += "</ul>"

		w.Header().Set("Content-Type", "text/html")
		w.Write([]byte(strings.Replace(uploadHTML, "[FILES]", fileText, 1)))
		return
	}

	if r.Method != "POST" {
		http.NotFound(w, r)
		return
	}

	file, fileInfo, err := r.FormFile("file")
	if err != nil {
		log.Println(err.Error())
		w.WriteHeader(500)
		return
	}
	defer file.Close()

	buffer := bytes.NewBuffer(nil)
	if _, err := io.Copy(buffer, file); err != nil {
		log.Println(err.Error())
		w.WriteHeader(400)
		return
	}
	ioutil.WriteFile(folder+fileInfo.Filename, buffer.Bytes(), 0644)

	w.WriteHeader(http.StatusAccepted)
}
