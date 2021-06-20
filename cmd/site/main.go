package main

import (
	"log"
	"os"

	"github.com/ChrisPritchard/GrislyGrotto/internal"
	_ "github.com/mattn/go-sqlite3"
)

func main() {
	log.SetFlags(0)
	log.SetOutput(os.Stdout)

	internal.StartServer()
}
