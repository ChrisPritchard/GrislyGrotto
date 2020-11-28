package main

import (
	"bytes"
	"encoding/base64"
	"io"
	"mime/multipart"
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

	headers := w.Header()

	bytes, exists, err := retrieveStorageFile(profile)
	if err != nil || !exists {
		bytes, _ = base64.StdEncoding.DecodeString("R0lGODlhAQABAAD/ACwAAAAAAQABAAACADs=") // 26 byte gif
		headers.Set("Content-Type", "image/gif")
		headers.Set("X-Content-Type", "image/gif")
	} else {
		mimeType := http.DetectContentType(bytes)
		headers.Set("Content-Type", mimeType)
		headers.Set("X-Content-Type", mimeType)
	}

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

	if len(username) > 20 || len(password) > 100 {
		renderView(w, r, loginViewModel{"Excessively sized values submitted"}, "login.html", "Login")
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

	err = setEncryptedCookie("user", username, mainDomain(r), authSessionExpiry, w)
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

	setCookie("user", "", mainDomain(r), time.Unix(0, 0), w)

	path := ""
	returnURI := r.URL.Query()["returnUrl"]
	if len(returnURI) > 0 && !strings.HasPrefix(returnURI[0], "editor") && !strings.HasPrefix(returnURI[0], "account-details") {
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
			errorMessage := tryUpdateDisplayName(username, displayName)
			if errorMessage != "" {
				model.DisplayNameError = errorMessage
			} else {
				model.DisplayNameSuccess = true
			}
		}

		if oldPassword != "" && newPassword != "" && newPasswordConfirm != "" {
			errorMessage := tryUpdatePassword(username, oldPassword, newPassword, newPasswordConfirm)
			if errorMessage != "" {
				model.PasswordError = errorMessage
			} else {
				model.PasswordSuccess = true
			}
		}

		file, fileHeader, err := r.FormFile("profileImage")
		if err == nil {
			defer file.Close()
			errorMessage := tryUpdateProfileImage(username, file, fileHeader)
			if errorMessage != "" {
				model.ImageError = errorMessage
			} else {
				model.ImageSuccess = true
			}
		}
	}

	renderView(w, r, model, "accountDetails.html", "Account Details")
}

func tryUpdateDisplayName(username string, displayName string) string {
	if len(displayName) > maxDisplayNameLength {
		return "Display name must be shorter than " + strconv.Itoa(maxDisplayNameLength) + " characters"
	}

	err := insertOrUpdateUser(username, "", displayName)
	if err != nil {
		return "Failed to update username"
	}

	return ""
}

func tryUpdatePassword(username, oldPassword, newPassword, newPasswordConfirm string) string {
	if len(newPassword) > maxPasswordLength || len(newPasswordConfirm) > maxPasswordLength || len(oldPassword) > maxCommentLength {
		return "A value was provided longer than the max password length of " + strconv.Itoa(maxPasswordLength)
	}

	if newPassword == oldPassword {
		return "New password cannot be the same as the old password"
	}

	if newPassword != newPasswordConfirm {
		return "New password does not match confirm new password"
	}

	if len(newPassword) < 14 {
		return "New password must be at least 14 characters long"
	}

	if valid, err := validateUser(username, oldPassword); !valid || err != nil {
		return "Current password is invalid"
	}

	err := insertOrUpdateUser(username, newPassword, "")
	if err != nil {
		return "Failed to update password"
	}

	return ""
}

func tryUpdateProfileImage(username string, file multipart.File, fileHeader *multipart.FileHeader) string {
	if fileHeader.Size > maxFileSize {
		return "File size exceeds maximum"
	}

	buffer := bytes.NewBuffer(nil)
	if _, err := io.Copy(buffer, file); err != nil {
		return "Unable to read file"
	}

	mimeType := http.DetectContentType(buffer.Bytes())
	if !strings.HasPrefix(mimeType, "image/") {
		return "File is not a valid image"
	}

	error := uploadStorageFile(username, buffer)
	if error != nil {
		return "There was an error uploading the file"
	}

	return ""
}
