# GrislyGrotto

The is my personal blogging website, where I mainly blog about games I play, books I read, stuff I write and and my boring little life. My friend Pete also blogs here, where he mostly talks about Doctor Who, writing and other stuff.

However, content aside, the main point of this blog for me is as a kata-like exercise of website development. Almost every single version has been a complete rebuild in a new technology stack, or a refinement / alteration of an existing technology stack.

To date there have been ASP.NET WebForms versions (unfortunately the code for those has been lost), MVC, Node.JS, F#, XSLT view-based transforms, paid-for hosting, Azure hosting, Raspberry Pi hosting, Go, SQL Server, SQLite, MySql, document storage, XML storage etc. Building a complete website with code, styling, persistence, authentication, hosting, ssl and domain names etc by yourself is a strong educational experience and I recommend anyone who is in or adjacent to web development have a go at doing this.

## Release 17

__Release Date:__ TBD

__Release Post:__ TBD

__Technology:__ Go

Also uses the following Go packages (outside of built-in go packages): [mattn/go-sqlite3](https://github.com/mattn/go-sqlite3) and [yuin/goldmark](https://github.com/yuin/goldmark) for markdown.

__Data Store:__ SQLite

__Dev Tool(s):__ VS Code

VS Code was used in three different ways:

- via Remote WSL (Windows 10 onto Ubuntu 18.04 WSL)
- on OSX
- on Windows 10 directly with mingw-x64 for cGo

The Go connection to SQlite was the biggest barrier, as it requires CGO, which requires GCC in path. Fine on linux/osx, harder on windows. Ultimately [tdm-gcc](http://tdm-gcc.tdragon.net/) was the easiest way to install this, and I recommend it. WSL2 remote worked great too, except that debugging didn't work (not as bad as it sounds for most of the development work).

Overall, this build was particularly enjoyable. Go is quite a pleasure to work with, something I found surprising coming from much higher-level functional languages. Simplicity has its merits.

## Usage notes

The site needs to be colocated with various static resources under the /static folder (just images, css and js, mainly). All templates are loaded in to Go via `go generate`.

To run the site, it requires the port and database connection name. These can be specified via the command line (use `-h` to get details). They can also be specified via environment variables, using the same env vars as a standard ASPNET Core site for legacy reasons (and the lulz): `ASPNETCORE_URLS` and `ConnectionStrings__Default`.

Remaining work:

```
recapcha? custom recapcha?
csrf tokens
returnurl for login with open redirect protection
rolling cookie expiry? expiry time in cookie val?
possible api integration with azure blob or backend api for image storage
```