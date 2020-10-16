package main

import (
	"net/http"
	"strconv"
)

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

	err = setEncryptedCookie("user", username, w)
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
