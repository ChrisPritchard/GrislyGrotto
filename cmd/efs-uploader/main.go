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
	listFileHTML   = "<li>%s - %d bytes&nbsp;<form method='post' style='display: inline'><input type='hidden' name='ACCESSKEY' value='%s' /><input type='hidden' name='todelete' value='%s' /><input type='submit' value='Delete' /></form></li>"
	uploadHTML     = "<!doctype html><html><head><meta charset='utf-8'></head></body><div>[FILES]</div><form method='POST' enctype='multipart/form-data'><input type='file' name='file' /><input type='submit' value='submit' /><br/><label><input type='checkbox' name='append' />&nbsp;append</label></form></body></html>"
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

	folder := os.Getenv(defaultPathEnv)
	if !strings.HasSuffix(folder, "/") {
		folder += "/"
	}

	if r.Method == "GET" {
		files, err := ioutil.ReadDir(folder)
		if err != nil {
			log.Println(err.Error())
			http.Error(w, "failed to read directory on efs with error: "+err.Error(), http.StatusBadRequest)
			return
		}

		fileText := "<ul>"
		if r.URL.Query().Get("success") == "true" {
			fileText = "<div>file uploaded successfully</div>" + fileText
		} else if r.URL.Query().Get("delete") == "true" {
			fileText = "<div>file deleted successfully</div>" + fileText
		}

		for _, v := range files {
			fileText += fmt.Sprintf(listFileHTML, folder+v.Name(), v.Size(), os.Getenv(authAccessKey), v.Name())
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

	toDelete := r.FormValue("todelete")
	if toDelete != "" {
		err := os.Remove(folder + toDelete)
		if err != nil {
			log.Println(err.Error())
			http.Error(w, "failed to delete file with error: "+err.Error(), http.StatusBadRequest)
			return
		}
		http.Redirect(w, r, "/?ACCESSKEY="+os.Getenv(authAccessKey)+"&delete=true", http.StatusFound)
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

	if r.FormValue("append") == "on" {
		file, err := os.OpenFile(folder+fileInfo.Filename, os.O_APPEND|os.O_CREATE|os.O_WRONLY, 0644)
		if err != nil {
			log.Println(err.Error())
			http.Error(w, "failed to open file on EFS with error: "+err.Error(), http.StatusBadRequest)
			return
		}

		defer file.Close()
		if _, err := file.Write(buffer.Bytes()); err != nil {
			log.Println(err.Error())
			http.Error(w, "failed to append data to file on EFS with error: "+err.Error(), http.StatusBadRequest)
			return
		}
	} else if err := os.WriteFile(folder+fileInfo.Filename, buffer.Bytes(), 0644); err != nil {
		log.Println(err.Error())
		http.Error(w, "failed to write file to EFS with error: "+err.Error(), http.StatusBadRequest)
		return
	}

	http.Redirect(w, r, "/?ACCESSKEY="+os.Getenv(authAccessKey)+"&success=true", http.StatusFound)
}
