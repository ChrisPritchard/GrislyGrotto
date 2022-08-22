package config

import (
	"database/sql"
	"time"

	"github.com/ChrisPritchard/GrislyGrotto/pkg/argon2"

	"github.com/yuin/goldmark"
	"github.com/yuin/goldmark/extension"
	"github.com/yuin/goldmark/renderer/html"
)

// globals and constants used in multiple places
// most globals are set early in main

var Secret [16]byte
var ConnectionString string
var ListenURL string
var ContentStorageName string

const defaultConnectionString = "./grislygrotto.db"
const defaultListenAddr = ":3000"
const defaultStorageName = "grislygrotto-content"

const envDatabaseKey = "GRISLYGROTTO_DB"
const envUrlKey = "GRISLYGROTTO_URL"
const envStorageKey = "GRISLYGROTTO_STORAGE"
const envSecretKey = "GRISLYGROTTO_SECRET"

var Database *sql.DB
var AuthenticatedUser = struct{}{}

const PageLength = 5
const MaxCommentCount = 20
const MaxSearchResults = 50
const SearchStripPad = 20

const MinWordCount = 100
const MaxFileSize = 1024 * 1500 // slightly higher than the JS will allow, to deal with chaos monkey

const AuthSessionExpiry = time.Hour
const ThemeExpiry = time.Hour * 8760 * 10         // ten years
const CommentAuthorityExpiry = time.Hour * 24 * 5 // five days
const MaxOwnedComments = 20

const MaxDisplayNameLength = 30
const MaxPasswordLength = 100
const MaxSearchTermLength = 100
const MaxCommentLength = 1000
const MaxTitleLength = 100

var ValidUploadExtensions = []string{".png", ".bmp", ".jpg", ".jpeg", ".gif", ".mp4", ".zip"}
var VideoExtensions = []string{".mp4"}
var DownloadExtensions = []string{".zip"}

// any changes should be replicated in the create author cmd tool
var PasswordConfig = &argon2.Argon2Config{
	Time:    1,
	Memory:  64 * 1024,
	Threads: 4,
	KeyLen:  32,
}

var MarkdownFull = goldmark.New(
	goldmark.WithExtensions(
		extension.GFM,
	),
	goldmark.WithRendererOptions(
		html.WithUnsafe(),
	),
)

var MarkdownRestricted = goldmark.New(
	goldmark.WithExtensions(
		extension.Linkify,
		extension.Strikethrough,
	),
)

var Months = []string{
	"", "January", "February", "March", "April", "May", "June",
	"July", "August", "September", "October", "November", "December"}
var MonthIndexes = map[string]string{
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

const BlockTime = 5 // seconds

const DraftPrefix = "[DRAFT] "
