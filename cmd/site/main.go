package main

import (
	"log"
	"net/http"
	"os"

	"github.com/ChrisPritchard/GrislyGrotto/internal"
	"github.com/ChrisPritchard/GrislyGrotto/internal/config"
	_ "github.com/mattn/go-sqlite3"
)

func main() {
	log.SetFlags(0)
	log.SetOutput(os.Stdout)

	proceed := config.ParseArgs() // setup globals from cmd line flags and files
	if !proceed {
		return
	}

	server := internal.CreateGlobalHandler()

	log.Printf("The Grisly Grotto has started!\nlistening locally at port %s\n", config.ListenURL)
	log.Println(http.ListenAndServe(config.ListenURL, server))
}
