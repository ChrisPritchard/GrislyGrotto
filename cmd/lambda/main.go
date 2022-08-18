package main

import (
	"log"
	"os"

	"github.com/ChrisPritchard/GrislyGrotto/internal"
	"github.com/ChrisPritchard/GrislyGrotto/internal/config"
	"github.com/ChrisPritchard/GrislyGrotto/pkg/lambda"
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
	lambda.Start(server)
}
