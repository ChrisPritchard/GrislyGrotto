# Database instructions

The database for the Grisly Grotto is simple: its a sqlite3 database with three tables. The following script will set it up from scratch when run against a blank db:

```sql
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
DELETE FROM sqlite_sequence;
INSERT INTO sqlite_sequence VALUES('Comments',5591);
CREATE INDEX "IX_Comments_Post_Key" ON "Comments" ("Post_Key");
CREATE INDEX "IX_Posts_Author_Username" ON "Posts" ("Author_Username");
COMMIT;
```

## Password column value generation

The most complicated part is creating the password for authors, which follows the following algorithm:

1. A random salt is generated (typically I use a GUID).
2. This is combined with the users password, e.g. if the password is `mypassword` and the salt is `arandomsalt` then the combined value is `arandomsaltmypassword`.
3. Using SHA384, hash the combined value then encode the result as base64. The previous example would be: `W7168U1nHuIYSHPQdeuCIHJlrXi0RASehZZonLqdaILi/bNHBDLPYLQTHgLA3EYA`
4. Append the salt to the final value with a quote, and store that. So the final value stored for the above example would be: `W7168U1nHuIYSHPQdeuCIHJlrXi0RASehZZonLqdaILi/bNHBDLPYLQTHgLA3EYA,arandomsalt`

The above can be done using the following go code:

```go
package main

import (
	"crypto/sha512"
	"encoding/base64"
	"fmt"
)

func main() {
	salt := "arandomsalt"
	password := "mypassword"

	toCheck := []byte(salt + password)
	hasher := sha512.New384()
	hasher.Write(toCheck)
	result := base64.StdEncoding.EncodeToString(hasher.Sum(nil))

	fmt.Println(result)
}
```

Notably, you can check the way passwords are checked via the `getUser` function within [data.go](./src/data.go).

## Is this secure?

Not really. 

If the database is captured, then SHA384 isn't tough to calculate with something like hashcat. The use of a random salt means a user can't use a rainbow table, and would have to brute force each password individually, but that only becomes a problem if you have thousands or hundreds of thousands of users to force. 

For this site, where there is only one or two users, having the salt at hand would mean the password could probably be bruteforced in seconds using something like rockyou, if the user is using a weak password. The site itself uses a simple brute force protection on its login form, plus https, that will defeat most web based password attacks, however best not to leak the database. 

The measures used here with salting and hashing are more of a proof of concept: this level of password storage obfuscation is 'reasonable' for regular, professional work, though at the time of writing you would likely use an algorithm that is expensive computationally like argon2 (which I am not doing, as it requires additional dependencies).