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

Passwords are stored in the database using the [argon2](https://en.wikipedia.org/wiki/Argon2) hashing algorithm, which is (with appropriate configuration) one of the strongest modern hashing algorithms

To generate a new author, or to update an existing author, run the site executable with the following args (assuming the site has been compiled as `grislygrotto`).

`./grislygrotto setauthor [username] [password] [displayname]`

This will take the password, run it through the argon2 algorithm, and set the result into the Author table.

Configuration params for the argon2 algorithm are stored in [globals.go](./src/globals.go).