package main

import (
	"bytes"
	"io"
	"io/ioutil"
	"log"
	"net/http"
	"os"
	"strings"

	"github.com/ChrisPritchard/GrislyGrotto/pkg/lambda"
)

const (
	authUserEnv = "USERNAME"
	authPassEnv = "PASSWORD"
	defaultPath = "UPLOADPATH"
	uploadHTML  = "<html><form method='POST'><input type='file' name='file' /></form></html>"
)

func main() {
	log.SetFlags(0)
	log.SetOutput(os.Stdout)

	lambda.Start(http.HandlerFunc(contentHandler))
}

func validateBasicAuth(w http.ResponseWriter, r *http.Request) bool {
	username, password, ok := r.BasicAuth()
	if !ok {
		w.Header().Set("WWW-Authenticate", "Basic realm=\"grislygrotto.nz\"")
		http.Error(w, "missing credentials", http.StatusUnauthorized)
		return false
	}
	if username != os.Getenv(authUserEnv) || password != os.Getenv(authPassEnv) {
		w.Header().Set("WWW-Authenticate", "Basic realm=\"grislygrotto.nz\"")
		http.Error(w, "invalid credentials", http.StatusUnauthorized)
		return false
	}
	return true
}

func contentHandler(w http.ResponseWriter, r *http.Request) {
	validateBasicAuth(w, r)

	if r.Method == "GET" {
		w.Header().Add("Content-Type", "text/html")
		w.Write([]byte(uploadHTML))
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

	folder := os.Getenv(defaultPath)
	if !strings.HasSuffix(folder, "/") {
		folder += "/"
	}
	ioutil.WriteFile(folder+fileInfo.Filename, buffer.Bytes(), 0644)

	w.WriteHeader(http.StatusAccepted)
}
