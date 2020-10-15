package main

import (
	"database/sql"
	"time"

	"github.com/yuin/goldmark"
	"github.com/yuin/goldmark/extension"
	"github.com/yuin/goldmark/renderer/html"
)

// globals and constants used in multiple places
// most globals are set early in main

var secret []byte
var connectionString string
var listenURL string
var isDevelopment bool
var contentStorageName string

const defaultConnectionString = "./grislygrotto.db"
const defaultListenAddr = ":3000"
const defaultStorageName = "grislygrotto-content"

var database *sql.DB
var authenticatedUser = struct{}{}

const pageLength = 5
const maxCommentCount = 20
const maxSearchResults = 50
const searchStripPad = 20
const cookieAge = time.Hour
const themeExpiry = time.Hour * 8760 * 10 // ten years
const minWordCount = 100
const maxFileSize = 1024 * 1000

var validUploadExtensions = []string{".png", ".bmp", ".jpg", ".jpeg", ".gif"}

var passwordConfig = &argon2Config{
	time:    1,
	memory:  64 * 1024,
	threads: 4,
	keyLen:  32,
}

var markdownEngine = goldmark.New(
	goldmark.WithExtensions(
		extension.GFM,
	),
	goldmark.WithRendererOptions(
		html.WithUnsafe(),
	),
)

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
