package main

import (
	"crypto/aes"
	"crypto/cipher"
	"crypto/rand"
	"encoding/base64"
	"errors"
	"fmt"
	"io"
	"io/ioutil"
	"log"
	"net/http"
	"strings"
	"time"

	_ "github.com/mattn/go-sqlite3"
)

var secret []byte
var compiledViews views

func main() {
	var err error
	secret, err = ioutil.ReadFile("./.secret")
	if err != nil {
		log.Fatal(err)
	}

	compiledViews = compileViews()
	setupRoutes()

	log.Printf("listening locally on port :%d\n", listenPort)
	log.Println(http.ListenAndServe(fmt.Sprintf(":%d", listenPort), nil))
}

func setupRoutes() {
	http.Handle("/static/",
		http.StripPrefix("/static/",
			http.FileServer(http.Dir("static"))))

	http.HandleFunc("/", latestPostsHandler)
	http.HandleFunc("/post/", singlePostHandler)
	http.HandleFunc("/delete-comment/", deleteCommentHandler)
	http.HandleFunc("/delete-post/", deletePostHandler)
	http.HandleFunc("/archives", archivesHandler)
	http.HandleFunc("/month/", monthHandler)
	http.HandleFunc("/search/", searchHandler)
	http.HandleFunc("/about", aboutHandler)
	http.HandleFunc("/login", loginHandler)
	http.HandleFunc("/editor/", editorHandler)
}

func setCookie(name, unencodedData string, w http.ResponseWriter) (err error) {
	data := time.Now().Format(time.RFC3339) + "|" + unencodedData
	cipher, err := encrypt([]byte(data), secret)
	log.Println(err)
	if err != nil {
		return err
	}
	b64 := base64.StdEncoding.EncodeToString(cipher)

	cookie := http.Cookie{
		Name:     name,
		Value:    b64,
		HttpOnly: true,
	}
	http.SetCookie(w, &cookie)
	return nil
}

func readCookie(name string, r *http.Request) (unencodedData string, err error) {
	cookie, err := r.Cookie(name)
	if err != nil {
		return "", err
	}

	b64 := cookie.Value

	cipher, err := base64.StdEncoding.DecodeString(b64)
	if err != nil {
		return "", err
	}

	dataB, err := decrypt(cipher, secret)
	if err != nil {
		return "", err
	}
	data := string(dataB)

	split := strings.Index(data, "|")
	if split == -1 {
		return "", errors.New("invalid cookie")
	}

	issued, err := time.Parse(time.RFC3339, data[:split])
	if err != nil {
		return "", errors.New("invalid cookie")
	}

	if time.Now().Sub(issued) > cookieAge {
		return "", errors.New("expired cookie")
	}

	return data[split+1:], nil
}

func encrypt(plaintext []byte, key []byte) ([]byte, error) {
	c, err := aes.NewCipher(key)
	if err != nil {
		return nil, err
	}

	gcm, err := cipher.NewGCM(c)
	if err != nil {
		return nil, err
	}

	nonce := make([]byte, gcm.NonceSize())
	if _, err = io.ReadFull(rand.Reader, nonce); err != nil {
		return nil, err
	}

	return gcm.Seal(nonce, nonce, plaintext, nil), nil
}

func decrypt(ciphertext []byte, key []byte) ([]byte, error) {
	c, err := aes.NewCipher(key)
	if err != nil {
		return nil, err
	}

	gcm, err := cipher.NewGCM(c)
	if err != nil {
		return nil, err
	}

	nonceSize := gcm.NonceSize()
	if len(ciphertext) < nonceSize {
		return nil, errors.New("ciphertext too short")
	}

	nonce, ciphertext := ciphertext[:nonceSize], ciphertext[nonceSize:]
	return gcm.Open(nil, nonce, ciphertext, nil)
}
