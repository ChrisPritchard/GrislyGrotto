package config

import (
	"crypto/rand"
	"database/sql"
	"flag"
	"log"
	"strings"
)

func ParseArgs() bool {
	ConnectionString = defaultConnectionString
	ListenURL = defaultListenAddr
	ContentStorageName = defaultStorageName

	connArg := flag.String("db", "", "the sqlite connection string\n\tdefaults to "+defaultConnectionString)
	urlArg := flag.String("url", "", "the url with port to listen to\n\tdefaults to "+defaultListenAddr)
	storageArg := flag.String("storage", "", "the target storage bucket/container for user content\n\tdefaults to "+defaultStorageName)
	flag.Parse()

	if *connArg != "" {
		ConnectionString = *connArg
	}

	db, err := sql.Open("sqlite3", ConnectionString) // db is closed by app close
	if err != nil {
		log.Fatal(err)
	}
	Database = db

	if *urlArg != "" {
		ListenURL = *urlArg
	}

	// handles if url is fully qualified with scheme, which
	// is invalid for Go's ListenAndServe
	portStart := strings.LastIndex(ListenURL, ":")
	if portStart > 0 {
		ListenURL = ListenURL[portStart:]
	} else if portStart < 0 {
		log.Fatal("invalid url specified - missing a :port")
	}

	secret := make([]byte, 16)
	rand.Read(secret)
	copy(Secret[:], secret)

	if *storageArg != "" {
		ContentStorageName = *storageArg
	}

	return true
}
