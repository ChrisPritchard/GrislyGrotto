package main

import (
	"database/sql"
	"flag"
	"log"
	"os"

	"github.com/ChrisPritchard/GrislyGrotto/pkg/argon2"
	_ "github.com/mattn/go-sqlite3"
)

var passwordConfig = &argon2.Argon2Config{
	Time:    1,
	Memory:  64 * 1024,
	Threads: 4,
	KeyLen:  32,
}

func main() {
	log.SetFlags(0)
	log.SetOutput(os.Stdout)

	connArg := flag.String("db", "", "the sqlite connection string")
	username := flag.String("username", "", "the username (for login) of the new/updated author")
	password := flag.String("password", "", "the password (for login) of the new/updated author")
	displayname := flag.String("displayname", "", "the display name (shown on posts) of the new/updated author")
	flag.Parse()

	if *connArg == "" || *username == "" || *password == "" || *displayname == "" {
		log.Println("Required arguments:")
		flag.PrintDefaults()
		os.Exit(1)
	}

	database, err := sql.Open("sqlite3", *connArg) // db is closed by app close
	if err != nil {
		log.Fatal(err)
	}

	_, err = database.Exec(initScript)
	if err != nil {
		log.Fatal(err)
	}

	err = insertOrUpdateUser(database, *username, *password, *displayname)
	if err != nil {
		log.Fatal(err)
	}

	log.Println("user created or updated successfully")
}

func insertOrUpdateUser(database *sql.DB, username, password, displayName string) error {
	var res sql.Result
	var err error
	var passwordHash string

	if password != "" {
		passwordHash, err = argon2.GenerateArgonHash(passwordConfig, password)
		if err != nil {
			return err
		}
	}

	if password == "" {
		res, err = database.Exec("UPDATE Authors SET DisplayName = ? WHERE Username = ?", displayName, username)
	} else if displayName != "" {
		res, err = database.Exec("UPDATE Authors SET Password = ?, DisplayName = ? WHERE Username = ?", passwordHash, displayName, username)
	} else {
		res, err = database.Exec("UPDATE Authors SET Password = ? WHERE Username = ?", passwordHash, username)
	}

	if err != nil {
		return err
	}
	rows, err := res.RowsAffected()
	if err != nil || rows == 1 {
		return err
	}

	_, err = database.Exec("INSERT INTO Authors (Username, Password, DisplayName) VALUES (?, ?, ?)", username, passwordHash, displayName)
	return err
}

var initScript = `
PRAGMA foreign_keys=OFF;
BEGIN TRANSACTION;
CREATE TABLE IF NOT EXISTS "Authors" (
    "Username" TEXT NOT NULL CONSTRAINT "PK_Authors" PRIMARY KEY,
    "Password" TEXT NULL,
    "DisplayName" TEXT NULL,
    "ImageUrl" TEXT NULL
);
CREATE TABLE IF NOT EXISTS "Posts" (
    "Key" TEXT NOT NULL CONSTRAINT "PK_Posts" PRIMARY KEY,
    "Title" TEXT NULL,
    "Author_Username" TEXT NULL,
    "Date" TEXT NOT NULL,
    "Content" TEXT NULL,
    "IsStory" INTEGER NOT NULL,
    "WordCount" INTEGER NOT NULL,
    CONSTRAINT "FK_Posts_Authors_Author_Username" FOREIGN KEY ("Author_Username") REFERENCES "Authors" ("Username") ON DELETE RESTRICT
);
CREATE TABLE IF NOT EXISTS "Comments" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Comments" PRIMARY KEY AUTOINCREMENT,
    "Post_Key" TEXT NULL,
    "Author" TEXT NULL,
    "Date" TEXT NOT NULL,
    "Content" TEXT NULL,
    CONSTRAINT "FK_Comments_Posts_Post_Key" FOREIGN KEY ("Post_Key") REFERENCES "Posts" ("Key") ON DELETE RESTRICT
);
CREATE INDEX IF NOT EXISTS "IX_Comments_Post_Key" ON "Comments" ("Post_Key");
CREATE INDEX IF NOT EXISTS "IX_Posts_Author_Username" ON "Posts" ("Author_Username");
COMMIT;
`
