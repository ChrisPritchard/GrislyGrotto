# GrislyGrotto

The is my personal blogging website, where I mainly blog about games I play, books I read, stuff I write and and my boring little life. My friend Pete also blogs here, where he mostly talks about Doctor Who, writing and other stuff.

However, content aside, the main point of this blog for me is as a kata-like exercise of website development. Almost every single version has been a complete rebuild in a new technology stack, or a refinement / alteration of an existing technology stack.

To date there have been ASP.NET WebForms versions (unfortunately the code for those has been lost), MVC, Node.JS, F#, XSLT view-based transforms, paid-for hosting, Azure hosting, Raspberry Pi hosting, Go, SQL Server, SQLite, MySql, document storage, XML storage etc. Building a complete website with code, styling, persistence, authentication, hosting, ssl and domain names etc by yourself is a strong educational experience and I recommend anyone who is in or adjacent to web development have a go at doing this.

Further details can be found in:

- [setup.md](./docs/setup.md) which details how the site should be built and deployed
- [updates.md](./docs/updates.md) which specifies how the site might be updated once deployed

## Release 17.5

__Release Date:__ 20/06/2021

__Release Post:__ [https://www.grislygrotto.nz/post/seventeen-point-five](https://www.grislygrotto.nz/post/seventeen-point-five)

__Technology:__ Go

Also uses the following Go packages (outside of built-in go packages):

- [mattn/go-sqlite3](https://github.com/mattn/go-sqlite3) for the database.
- [yuin/goldmark](https://github.com/yuin/goldmark) for markdown.
- [x/crypto/argon2](https://golang.org/x/crypto/argon2) for user password hashing.
- [aws/aws-sdk-go](https://github.com/aws/aws-sdk-go) for image storage.

Install these via `go mod download`.

__Data Store:__ SQLite

__Dev Tool(s):__ VS Code

The 17.5 update over 17 involved bumping the Go version to latest, and switching to the Go standard project structure which was quite extensive. The new Go Embed functionality was used to.

## All Releases of the Grisly Grotto

- [17.5 (Current) written in **Go**, 20th June 2021](https://github.com/ChrisPritchard/GrislyGrotto)
- [17.0, written in **Go**, 15th March 2020 (my daughter's 3rd birthday!)](https://github.com/ChrisPritchard/tree/release-17.0)
- [16.0, written in **F#** on .Net Core 2.2, 20th June 2019](https://github.com/ChrisPritchard/GrislyGrotto/tree/release-16.0)
- [15.0, written in **F#** on .NET Core 2.1, 20th June 2018](https://github.com/ChrisPritchard/GrislyGrotto/tree/release-15.0)
- [14.3, written in **C#** 6 on .NET Core 2.0, 5th December 2017](https://github.com/ChrisPritchard/GrislyGrotto/tree/release-14.3)
- [14.0, written in **C#** 6 on .NET Core 1.1, 2nd January 2017](https://github.com/ChrisPritchard/GrislyGrotto/tree/release-14.3)
- [13.0, written in **C#** on ASP.NET MVC 5, .NET Framework 4.5.2, 24th April 2015](https://github.com/ChrisPritchard/GrislyGrotto/tree/release-13.0)
- [12.0, written in **NodeJS**, 21st February 2014](https://github.com/ChrisPritchard/GrislyGrotto/tree/release-12.0)
- [11.0, written in **C#** on ASP.NET MVC 4, .NET Framework 4.5, 15th June 2013](https://github.com/ChrisPritchard/GrislyGrotto/tree/release-11.0)
- [10.5, written in **C#**, WCF and Custom JS SPA, 23rd January 2013](https://github.com/ChrisPritchard/GrislyGrotto/tree/release-10.5)
- [10.2, written in **C#**, WCF and Custom JS SPA, 4th August 2012](https://github.com/ChrisPritchard/GrislyGrotto/tree/release-10.5)
- [9.0, written in **C#**, Custom HTTP Handler, HTML5, 6th September 2011](https://github.com/ChrisPritchard/GrislyGrotto/tree/release-9.0)
- [8.6, written in **C#**, XSLT Transforms for HTML generation, 23rd February 2011](https://github.com/ChrisPritchard/GrislyGrotto/tree/release-8.6)
- [8.0, written in **C#**, XSLT Transforms for HTML generation, 20th February 2011](https://github.com/ChrisPritchard/GrislyGrotto/tree/release-8.0)
- [7.8, written in **C#**, XSLT Transforms for HTML generation, 17th April 2010](https://github.com/ChrisPritchard/GrislyGrotto/tree/release-7.8)
- [7.1, written in **C#**, XSLT Transforms for HTML generation, 6th January 2010](https://github.com/ChrisPritchard/GrislyGrotto/tree/release-7.8)
- [6.5, written in **C#**, ASP.NET MVC3 with XSLT View Engine, 28th July 2009](https://github.com/ChrisPritchard/GrislyGrotto/tree/release-6.5)
- [6.0, written in **C#**, ASP.NET MVC3 with XSLT View Engine, 2nd June 2009](https://github.com/ChrisPritchard/GrislyGrotto/tree/release-6.0)
- [5.0, written in **C#**, ASP.NET MVC1 with XSLT View Engine, 19th January 2009](https://github.com/ChrisPritchard/GrislyGrotto/tree/release-5.0)
- [4.5, written in **C#**, ASP.NET with XSLT, 29th September 2008](https://github.com/ChrisPritchard/GrislyGrotto/tree/release-4.5)
- [4.0, written in **C#**, ASP.NET with XSLT, 4th June 2008](https://github.com/ChrisPritchard/GrislyGrotto/tree/release-4.0)

Earlier versions have been lost to time, but were written in ASP.NET Webforms, mainly, .NET Framework 1.1 and 2.0.