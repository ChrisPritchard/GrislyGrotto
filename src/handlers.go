package main

import (
	"html/template"
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
				renderView(w, model, views.Latest)
			}
		}
	}
}

func singlePostHandler(views views) func(w http.ResponseWriter, r *http.Request) {
	return func(w http.ResponseWriter, r *http.Request) {
		key := r.URL.Path[len("/post/"):]
		post, pageNotFound, err := getSinglePost(key)
		if pageNotFound {
			http.NotFound(w, r)
		} else if err != nil {
			serverError(w, err)
		} else {
			if r.Method == "POST" {
				if len(post.Comments) >= maxCommentCount {
					badRequest(w, "max comments reached")
				} else {
					createComment(w, r, post.Key)
				}
			} else if r.Method == "GET" {
				model := singleViewModel{true, post}
				if len(post.Comments) == maxCommentCount {
					model.CanComment = false
				}
				renderView(w, model, views.Single)
			} else {
				http.NotFound(w, r)
			}
		}
	}
}

func createComment(w http.ResponseWriter, r *http.Request, postKey string) {
	err := r.ParseForm()
	if err != nil {
		serverError(w, err)
	}

	author, content := r.Form["author"], r.Form["content"]
	if len(author) != 1 || len(content) != 1 || areDangerous(author[0], content[0]) {
		badRequest(w, "both author and content are required and must be safe values")
	} else {
		err := addCommentToBlog(author[0], content[0], postKey)
		if err != nil {
			serverError(w, err)
		} else {
			http.Redirect(w, r, r.URL.Path, http.StatusFound)
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
			renderView(w, model, views.Archives)
		}
	}
}

func searchHandler(views views) func(w http.ResponseWriter, r *http.Request) {
	return func(w http.ResponseWriter, r *http.Request) {
		if r.Method != "GET" {
			http.NotFound(w, r)
		} else {
			searchParam, hasSearch := r.URL.Query()["searchTerm"]
			if !hasSearch || len(searchParam[0]) == 0 {
				renderView(w, nil, views.Search)
			} else {
				results, err := getSearchResults(searchParam[0])
				if err != nil {
					serverError(w, err)
				}
				zeroResults := len(results) == 0
				renderView(w, searchViewModel{searchParam[0], zeroResults, results}, views.Search)
			}
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

func renderView(w http.ResponseWriter, model interface{}, view *template.Template) {
	if err := view.ExecuteTemplate(w, "master", model); err != nil {
		serverError(w, err)
	}
}

func serverError(w http.ResponseWriter, err error) {
	http.Error(w, err.Error(), http.StatusInternalServerError)
}

func badRequest(w http.ResponseWriter, message string) {
	http.Error(w, message, http.StatusBadRequest)
}
