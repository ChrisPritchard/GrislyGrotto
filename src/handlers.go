package main

import (
	"net/http"
	"strconv"
	"strings"
)

func latestPostsHandler(views views) func(w http.ResponseWriter, r *http.Request) {
	return func(w http.ResponseWriter, r *http.Request) {
		if r.Method != "GET" {
			http.NotFound(w, r)
		} else {
			page, notFirstPage := getPageFromQuery(r)

			posts, err := getLatestPosts(page)
			if err != nil {
				serverError(w, err)
			} else {
				model := latestViewModel{notFirstPage, page - 1, page + 1, posts}
				renderView(w, r, model, views.Latest)
			}
		}
	}
}

func getPageFromQuery(r *http.Request) (page int, notFirstPage bool) {
	pageParam, hasPage := r.URL.Query()["page"]
	page = 0
	notFirstPage = false
	if hasPage && len(pageParam[0]) > 0 {
		page, _ = strconv.Atoi(pageParam[0])
		if page < 0 {
			page = 0
		}
		if page != 0 {
			notFirstPage = true
		}
	}
	return page, notFirstPage
}

func singlePostHandler(views views) func(w http.ResponseWriter, r *http.Request) {
	return func(w http.ResponseWriter, r *http.Request) {
		key := r.URL.Path[len("/post/"):]
		post, pageNotFound, err := getSinglePost(key)
		if pageNotFound {
			http.NotFound(w, r)
			return
		}

		if err != nil {
			serverError(w, err)
			return
		}

		currentUser, err := readCookie("user", r)
		ownBlog := err == nil && currentUser == post.AuthorUsername

		if r.Method == "POST" {
			if len(post.Comments) >= maxCommentCount {
				badRequest(w, "max comments reached")
				return
			}

			commentError, err := createComment(r, post.Key)
			if err != nil {
				serverError(w, err)
			}
			model := singleViewModel{post, ownBlog, true, commentError}
			if len(post.Comments)+1 >= maxCommentCount {
				model.CanComment = false
			}
			renderView(w, r, model, views.Single)
		} else if r.Method == "GET" {
			model := singleViewModel{post, ownBlog, true, ""}
			if len(post.Comments) >= maxCommentCount {
				model.CanComment = false
			}
			renderView(w, r, model, views.Single)
		} else {
			http.NotFound(w, r)
		}
	}
}

func createComment(r *http.Request, postKey string) (commentError string, err error) {
	err = r.ParseForm()
	if err != nil {
		return "", err
	}

	author, content := r.Form["author"], r.Form["content"]
	if len(author) != 1 || len(author[0]) == 0 || len(content) != 1 || len(content[0]) == 0 || areDangerous(author[0], content[0]) {
		return "both author and content are required and must be safe values", nil
	}

	err = addCommentToBlog(author[0], content[0], postKey)
	if err != nil {
		return "", err
	}
	return "", nil
}

func deleteCommentHandler(w http.ResponseWriter, r *http.Request) {
	if r.Method != "POST" {
		http.NotFound(w, r)
		return
	}

	id := r.URL.Path[len("/delete-comment/"):]
	idN, err := strconv.Atoi(id)
	if err != nil {
		badRequest(w, "invalid comment id")
		return
	}

	user, err := readCookie("user", r)
	if err != nil {
		unauthorised(w)
		return
	}

	success, err := deleteComment(idN, user)
	if err != nil {
		serverError(w, err)
	} else if !success {
		unauthorised(w)
	} else {
		postKey := r.URL.Query()["postKey"]
		if len(postKey) != 0 && len(postKey[0]) > 0 {
			http.Redirect(w, r, "/post/"+postKey[0]+"#comments", http.StatusFound)
		} else {
			http.Redirect(w, r, "/", http.StatusFound)
		}
	}
}

func archivesHandler(views views) func(w http.ResponseWriter, r *http.Request) {
	return func(w http.ResponseWriter, r *http.Request) {
		if r.Method != "GET" {
			http.NotFound(w, r)
		} else {
			yearSets, err := getYearMonthCounts()
			if err != nil {
				serverError(w, err)
			}
			stories, err := getStories()
			if err != nil {
				serverError(w, err)
			}
			model := archivesViewModel{yearSets, stories}
			renderView(w, r, model, views.Archives)
		}
	}
}

func monthHandler(views views) func(w http.ResponseWriter, r *http.Request) {
	return func(w http.ResponseWriter, r *http.Request) {
		if r.Method != "GET" {
			http.NotFound(w, r)
		} else {
			token := r.URL.Path[len("/month/"):]
			split := strings.Index(token, "/")
			if len(token) == 0 || split == -1 {
				http.NotFound(w, r)
			}
			month, year := token[:split], token[split+1:]
			if monthIndex(month) == -1 {
				http.NotFound(w, r)
			}
			yearN, err := strconv.Atoi(year)
			if err != nil || yearN < 2006 || yearN > 2100 {
				http.NotFound(w, r)
			}

			posts, err := getPostsForMonth(month, year)
			if err != nil {
				serverError(w, err)
			}
			prevMonth, prevYear := getPrevMonth(month, yearN)
			nextMonth, nextYear := getNextMonth(month, yearN)
			model := monthViewModel{month, year, prevMonth, prevYear, nextMonth, nextYear, posts}
			renderView(w, r, model, views.Month)
		}
	}
}

func getPrevMonth(month string, year int) (string, string) {
	if strings.ToLower(month) == "january" {
		return "December", strconv.Itoa(year - 1)
	}
	index := monthIndex(month)
	return months[index-1], strconv.Itoa(year)
}

func getNextMonth(month string, year int) (string, string) {
	if strings.ToLower(month) == "december" {
		return "January", strconv.Itoa(year + 1)
	}
	index := monthIndex(month)
	return months[index+1], strconv.Itoa(year)
}

func monthIndex(month string) int {
	for i, m := range months {
		if strings.ToLower(m) == strings.ToLower(month) {
			return i
		}
	}
	return -1
}

func searchHandler(views views) func(w http.ResponseWriter, r *http.Request) {
	return func(w http.ResponseWriter, r *http.Request) {
		if r.Method != "GET" {
			http.NotFound(w, r)
		} else {
			searchParam, hasSearch := r.URL.Query()["searchTerm"]
			if !hasSearch || len(searchParam[0]) == 0 {
				renderView(w, r, nil, views.Search)
			} else {
				results, err := getSearchResults(searchParam[0])
				if err != nil {
					serverError(w, err)
				}
				zeroResults := len(results) == 0
				renderView(w, r, searchViewModel{searchParam[0], zeroResults, results}, views.Search)
			}
		}
	}
}

func aboutHandler(views views) func(w http.ResponseWriter, r *http.Request) {
	return func(w http.ResponseWriter, r *http.Request) {
		if r.Method != "GET" {
			http.NotFound(w, r)
		} else {
			renderView(w, r, nil, views.About)
		}
	}
}

func loginHandler(views views) func(w http.ResponseWriter, r *http.Request) {
	return func(w http.ResponseWriter, r *http.Request) {
		if r.Method == "POST" {
			err := r.ParseForm()
			if err != nil {
				serverError(w, err)
			}

			username, password := r.Form["username"], r.Form["password"]
			if len(username) != 1 || len(password) != 1 {
				renderView(w, r, loginViewModel{"both username and password are required"}, views.Login)
			} else {
				user, err := getUser(username[0], password[0])
				if err != nil {
					renderView(w, r, loginViewModel{"invalid credentials"}, views.Login)
				} else {
					err = setCookie("user", user, w)
					if err != nil {
						serverError(w, err)
					}
					http.Redirect(w, r, "/", http.StatusFound)
				}
			}
		} else if r.Method == "GET" {
			renderView(w, r, loginViewModel{""}, views.Login)
		} else {
			http.NotFound(w, r)
		}
	}
}

func areDangerous(values ...string) bool {
	for _, v := range values {
		if strings.ContainsAny(v, "<>") {
			return true
		}
	}
	return false
}

func serverError(w http.ResponseWriter, err error) {
	http.Error(w, err.Error(), http.StatusInternalServerError)
}

func badRequest(w http.ResponseWriter, message string) {
	http.Error(w, message, http.StatusBadRequest)
}

func unauthorised(w http.ResponseWriter) {
	http.Error(w, "unauthorised", http.StatusUnauthorized)
}
