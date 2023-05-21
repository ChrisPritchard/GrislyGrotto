# GrislyGrotto

The is my personal blogging website, where I mainly blog about games I play, books I read, stuff I write and and my boring little life. My friend Pete also blogs here, where he mostly talks about Doctor Who, writing and other stuff.

However, content aside, the main point of this blog for me is as a kata-like exercise of website development. Almost every single version has been a complete rebuild in a new technology stack, or a refinement / alteration of an existing technology stack.

To date there have been ASP.NET WebForms versions (unfortunately the code for those has been lost), MVC, Node.JS, F#, XSLT view-based transforms, paid-for hosting, Azure hosting, Raspberry Pi hosting, **Rust**, Go, SQL Server, SQLite, MySql, document storage, XML storage etc. Building a complete website with code, styling, persistence, authentication, hosting, ssl and domain names etc by yourself is a strong educational experience and I recommend anyone who is in or adjacent to web development have a go at doing this.

Further details can be found in:

- [setup.md](./docs/setup.md) which details how the site should be built and deployed
- [updates.md](./docs/updates.md) which specifies how the site might be updated once deployed

## Release 18.0

__Release Date:__ 22/05/2023

__Release Post:__ https://grislygrotto.nz/post/grisly-grotto-180---rust-edition

__Technology:__ Rust

See all crates used in [the cargo file](./Cargo.toml)

__Data Store:__ SQLite

__Dev Tool(s):__ VS Code

18.0 is the first version to use Rust (following my personal development history, C# -> NodeJS -> C# -> F# -> Go -> Rust). Design emphasis was on size, performance and being self-contained: notably, though the web framework used is actix-web which by default has external file templates, all templates including all static files like JavaScript, images, CSS etc are embedded in the binary. This makes it a single file deployment... well, single file plus the SQLite database file. I had this in the prior Go version too and really like it for my needs.

## All Releases of the Grisly Grotto

- 18, written in **Rust**, 22nd of May, 2023
- [17.6, written in **Go**, 12th September 2022](https://github.com/ChrisPritchard/GrislyGrotto/tree/release-17-final)
- [17.5, written in **Go**, 20th June 2021](https://github.com/ChrisPritchard/GrislyGrotto/tree/release-17.5)
- [17.0, written in **Go**, 15th March 2020 (my daughter's 3rd birthday!)](https://github.com/ChrisPritchard/tree/release-17.0)
- [16.0, written in **F#** on .NET Core 2.2, 20th June 2019](https://github.com/ChrisPritchard/GrislyGrotto/tree/release-16.0)
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