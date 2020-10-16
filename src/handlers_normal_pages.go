package main

import (
	"net/http"
	"strconv"
	"strings"
)

func latestPostsHandler(w http.ResponseWriter, r *http.Request) {
	if r.Method != "GET" {
		http.NotFound(w, r)
		return
	}

	page, notFirstPage := getPageFromQuery(r)
	posts, err := getLatestPosts(page, getCurrentUser(r))
	if err != nil {
		serverError(w, err)
		return
	}

	model := latestViewModel{notFirstPage, page - 1, page + 1, posts}
	renderView(w, r, model, "latest.html", "Latest Posts")
}

func getPageFromQuery(r *http.Request) (page int, notFirstPage bool) {
	pageParam := r.URL.Query().Get("page")
	page = 0
	notFirstPage = false
	if pageParam != "" {
		page, _ = strconv.Atoi(pageParam)
		if page < 0 {
			page = 0
		}
		if page != 0 {
			notFirstPage = true
		}
	}
	return page, notFirstPage
}

func archivesHandler(w http.ResponseWriter, r *http.Request) {
	if r.Method != "GET" {
		http.NotFound(w, r)
		return
	}

	currentUser := getCurrentUser(r)

	yearSets, err := getYearMonthCounts(currentUser)
	if err != nil {
		serverError(w, err)
		return
	}

	stories, err := getStories(currentUser)
	if err != nil {
		serverError(w, err)
		return
	}

	model := archivesViewModel{yearSets, stories}
	renderView(w, r, model, "archives.html", "Archives")
}

func monthHandler(w http.ResponseWriter, r *http.Request) {
	if r.Method != "GET" {
		http.NotFound(w, r)
		return
	}

	token := r.URL.Path[len("/archives/"):]
	split := strings.Index(token, "/")
	if len(token) == 0 || split == -1 {
		http.NotFound(w, r)
		return
	}

	month, year := token[:split], token[split+1:]
	if monthIndex(month) == -1 {
		http.NotFound(w, r)
		return
	}

	yearN, err := strconv.Atoi(year)
	if err != nil || yearN < 2006 || yearN > 2100 {
		http.NotFound(w, r)
		return
	}

	posts, err := getPostsForMonth(month, year, getCurrentUser(r))
	if err != nil {
		serverError(w, err)
		return
	}

	prevMonth, prevYear := getPrevMonth(month, yearN)
	nextMonth, nextYear := getNextMonth(month, yearN)
	model := monthViewModel{month, year, prevMonth, prevYear, nextMonth, nextYear, posts}
	renderView(w, r, model, "month.html", month+" "+year)
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

func searchHandler(w http.ResponseWriter, r *http.Request) {
	if r.Method != "GET" {
		http.NotFound(w, r)
		return
	}

	searchParam := r.URL.Query().Get("searchTerm")
	if searchParam == "" {
		renderView(w, r, nil, "search.html", "Search")
		return
	}

	results, err := getSearchResults(searchParam, getCurrentUser(r))
	if err != nil {
		serverError(w, err)
		return
	}

	zeroResults := len(results) == 0
	renderView(w, r, searchViewModel{searchParam, zeroResults, results}, "search.html", "Search")
}

func aboutHandler(w http.ResponseWriter, r *http.Request) {
	if r.Method != "GET" {
		http.NotFound(w, r)
		return
	}

	renderView(w, r, nil, "about.html", "About")
}
