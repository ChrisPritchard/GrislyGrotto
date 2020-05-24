package main

import "time"

// globals and constants used in multiple places
// most globals are set early in main

var secret []byte
var compiledViews views
var connectionString string
var listenURL string
var currentUser string

const defaultConnectionString = "./grislygrotto.db"
const defaultListenAddr = ":3000"

const pageLength = 5
const maxCommentCount = 20
const maxSearchResults = 50
const searchStripPad = 20
const cookieAge = time.Hour
const themeExpiry = time.Hour * 8760 * 10 // ten years
const minWordCount = 100

const markdownToken = "markdown|"

var months = []string{
	"", "January", "February", "March", "April", "May", "June",
	"July", "August", "September", "October", "November", "December"}
var monthIndexes = map[string]string{
	"January":   "01",
	"February":  "02",
	"March":     "03",
	"April":     "04",
	"May":       "05",
	"June":      "06",
	"July":      "07",
	"August":    "08",
	"September": "09",
	"October":   "10",
	"November":  "11",
	"December":  "12",
}

// used for brute force protection
var blocked = map[string]int64{}

const blockTime = 5 // seconds

const draftPrefix = "[DRAFT] "
