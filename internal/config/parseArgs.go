package config

import (
	"crypto/rand"
	"database/sql"
	"flag"
	"log"
	"os"
	"strings"
)

func ParseArgs() bool {
	ConnectionString = defaultConnectionString
	ListenURL = defaultListenAddr
	ContentStorageName = defaultStorageName

	// read from environment variables
	if envConn := os.Getenv(envDatabaseKey); envConn != "" {
		ConnectionString = envConn
	}
	if envUrl := os.Getenv(envUrlKey); envUrl != "" {
		ListenURL = envUrl
	}
	if envStorage := os.Getenv(envStorageKey); envStorage != "" {
		ContentStorageName = envStorage
	}

	// flags override env vars
	connArg := flag.String("db", "", "the sqlite connection string\n\tdefaults to "+defaultConnectionString)
	urlArg := flag.String("url", "", "the url with port to listen to\n\tdefaults to "+defaultListenAddr)
	storageArg := flag.String("storage", "", "the target storage bucket/container for user content\n\tdefaults to "+defaultStorageName)
	flag.Parse()

	if *connArg != "" {
		ConnectionString = *connArg
	}

	db, err := sql.Open("sqlite", ConnectionString) // db is closed by app close
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

	// for lambda hosting, the secret needs to be specified else it will be random on each request
	if envSecret := os.Getenv(envSecretKey); envSecret != "" {
		if len(envSecret) != len(secret) {
			log.Fatalf("environment secret should be %d characters long\n", len(secret))
		}
		secret = []byte(envSecret)
	} else {
		rand.Read(secret) // but for site hosting this will be generated once on site start
	}

	copy(Secret[:], secret)

	if *storageArg != "" {
		ContentStorageName = *storageArg
	}

	return true
}
