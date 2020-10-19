package main

import (
	"encoding/base64"
	"net/http"
	"strconv"
	"strings"
	"time"
)

func profileImageHandler(w http.ResponseWriter, r *http.Request) {
	if r.Method != "GET" {
		http.NotFound(w, r)
		return
	}

	profile := r.URL.Path[len("/profile-image/"):]
	if len(profile) == 0 {
		http.NotFound(w, r)
		return
	}

	bytes, exists, err := retrieveStorageFile(profile)
	if err != nil || !exists {
		bytes, _ = base64.StdEncoding.DecodeString("R0lGODlhAQABAAD/ACwAAAAAAQABAAACADs=") // 26 byte gif
	}

	setMimeType(w, r)
	w.Write(bytes)
}

func loginHandler(w http.ResponseWriter, r *http.Request) {
	if r.Method == "GET" {
		renderView(w, r, loginViewModel{""}, "login.html", "Login")
		return
	}

	if r.Method != "POST" {
		http.NotFound(w, r)
		return
	}

	username, password := r.FormValue("username"), r.FormValue("password")
	if username == "" || password == "" {
		renderView(w, r, loginViewModel{"Both username and password are required"}, "login.html", "Login")
		return
	}

	blockTime := getBlockTime(r, username)
	if blockTime > 0 {
		renderView(w, r, loginViewModel{"Cannot make another attempt for another " + strconv.Itoa(blockTime) + " seconds"}, "login.html", "Login")
		return
	}

	valid, err := validateUser(username, password)
	if err != nil || !valid {
		setBlockTime(r, username)
		renderView(w, r, loginViewModel{"Invalid credentials"}, "login.html", "login")
		return
	}

	err = setEncryptedCookie("user", username, authSessionExpiry, w)
	if err != nil {
		serverError(w, err)
		return
	}

	path := ""
	returnURI := r.URL.Query()["returnUrl"]
	if len(returnURI) > 0 {
		path = returnURI[0]
	}
	http.Redirect(w, r, "/"+path, http.StatusFound)
}

func logoutHandler(w http.ResponseWriter, r *http.Request) {
	if r.Method != "GET" {
		http.NotFound(w, r)
		return
	}

	setCookie("user", "", time.Unix(0, 0), w)

	path := ""
	returnURI := r.URL.Query()["returnUrl"]
	if len(returnURI) > 0 && !strings.HasPrefix(returnURI[0], "editor") {
		path = returnURI[0]
	}
	http.Redirect(w, r, "/"+path, http.StatusFound)
}

func accountDetailsHandler(w http.ResponseWriter, r *http.Request) {
	renderView(w, r, nil, "accountDetails.html", "Account Details")
}
