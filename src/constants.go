package main

import "time"

const defaultConnectionString = "./grislygrotto.db"
const defaultListenPort = "3000"

const pageLength = 5
const maxCommentCount = 20
const maxSearchResults = 50
const searchStripPad = 20
const cookieAge = time.Hour
const minWordCount = 100

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
