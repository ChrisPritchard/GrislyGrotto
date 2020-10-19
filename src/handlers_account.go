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
	if r.Method != "GET" && r.Method != "POST" {
		http.NotFound(w, r)
		return
	}

	currentUser := getCurrentUser(r)
	if currentUser == nil {
		unauthorised(w)
		return
	}

	username := *currentUser
	model := accountDetailsViewModel{username, "", "", false, "", false, "", false}

	var err error
	model.DisplayName, err = getDisplayName(username)
	if err != nil {
		serverError(w, err)
		return
	}

	if r.Method == "POST" {
		displayName, oldPassword, newPassword, newPasswordConfirm :=
			r.FormValue("displayName"), r.FormValue("oldPassword"),
			r.FormValue("newPassword"), r.FormValue("newPasswordConfirm")

		if displayName != "" {
			model.DisplayName = displayName
			if len(displayName) > 50 {
				model.DisplayNameError = "Display name must be shorter than 50 characters"
			} else {
				updateDisplayName(username, displayName)
				model.DisplayNameSuccess = true
			}
		}

		if oldPassword != "" && newPassword != "" && newPasswordConfirm != "" {
			if newPassword == oldPassword {
				model.PasswordError = "New password cannot be the same as the old password"
			} else if newPassword != newPasswordConfirm {
				model.PasswordError = "New password does not match confirm new password"
			} else if len(newPassword) < 14 {
				model.PasswordError = "New password must be at least 14 characters long (lol)"
			} else if valid, err := validateUser(username, oldPassword); !valid || err != nil {
				model.PasswordError = "Old password is invalid"
			} else {
				updatePassword(username, newPassword)
				model.PasswordSuccess = true
			}
		}

		file, fileHeader, err := r.FormFile("profileImage")
		if err == nil {
			defer file.Close()

			// validate file
			// upload and mark success
		}
	}

	renderView(w, r, model, "accountDetails.html", "Account Details")
}
