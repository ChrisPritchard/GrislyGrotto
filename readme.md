# GrislyGrotto

The is my personal blogging website, where I mainly blog about games I play, books I read, stuff I write and and my boring little life. My friend Pete also blogs here, where he mostly talks about Doctor Who, writing and other stuff.

However, content aside, the main point of this blog for me is as a kata-like exercise of website development. Almost every single version has been a complete rebuild in a new technology stack, or a refinement / alteration of an existing technology stack.

To date there have been ASP.NET WebForms versions (unfortunately the code for those has been lost), MVC, Node.JS, F#, XSLT view-based transforms, paid-for hosting, Azure hosting, Raspberry Pi hosting, Go, SQL Server, SQLite, MySql, document storage, XML storage etc. Building a complete website with code, styling, persistence, authentication, hosting, ssl and domain names etc by yourself is a strong educational experience and I recommend anyone who is in or adjacent to web development have a go at doing this.

## Release 17

__Release Date:__ TBD

__Release Post:__ TBD

__Frontend:__ Golang with SQLite extension and golang html templates.

__Data Store:__ SQLite

__Dev Tool(s):__ 

- VS Code via Remote WSL (Windows 10 onto Ubuntu 18.04 WSL)
- VS Code on OSX
- VS Code to local windows with mingw-x64 for cGo

The Go connection to SQlite was the biggest barrier, as it requires CGO, which requires GCC in path. Fine on linux/osx, harder on windows. Ultimately [tdm-gcc](http://tdm-gcc.tdragon.net/) was the easiest way to install this, and I recommend it. WSL2 remote worked great too, except that debugging didn't work (not as bad as it sounds for most of the development work).

Overall, this build was particularly enjoyable. Go is quite a pleasure to work with, something I found surprising coming from much higher-level functional languages. Simplicity has its merits.

Remaining work:

```
    recapcha? custom recapcha?
    csrf tokens
    markdown editor?
	returnurl for login with open redirect protection
    rolling cookie expiry? expiry time in cookie val?
	possible api integration with azure blob or backend api for image storage
```