package main

func loadTemplates() {
	templateContent["about.html"] = `{{ define "content" }}
<h2>About Me</h2>
<p>
My name is Christopher Pritchard, and I work as a security consultant in the infosec industry, based in Wellington, New Zealand. Specifically, I work for the excellent <a href="https://www.aurainfosec.com/">Aura Information Security</a>.
</p>
<p>
I also do development contracting on occasion through through my company, <a href="https://aquinas.nz">Aquinas IT</a>. 
In the past, I have worked for long periods as a full-time employee and as a contractor at various Wellington businesses.
</p>
<p>
I have been working in the IT industry for over fifteen years now, primary in development or development-related roles. While management, solution design and technical leadership have increasingly been part of my responsibilities, I always prefer to stay close to the code. My move into information security is the next step in my career, and I am enjoying it immensely so far.
</p>
<p>
Outside of work, my hobbies are writing (successive participant/winner in the NaNoWriMo competition each year I'm proud to say), and gaming. I also do a lot of personal projects, such as the one you are looking at now.
</p>
<p>
Finally, and by no means least, I am a happily married man with a young daughter, as well as a small dog.
</p>
<h2>Contact</h2>
<p>
I can be reached most easily by my personal email address: <a href="mailto:chrispritchard.nz@gmail.com">chrispritchard.nz@gmail.com</a>.<br/>
My LinkedIn profile is <a href="https://nz.linkedin.com/pub/christopher-pritchard/a/9b6/a66">here</a>.<br/>
My Github profile is <a href="https://github.com/ChrisPritchard">here</a>.
</p>
<h2>About the Grisly Grotto</h2>
<p>The Grisly Grotto is the personal blogging site of myself and my friend/uncle, Peter Coleman. The name comes from a computer game, <a href="https://en.wikipedia.org/wiki/Quake_(video_game)">Quake</a> (from 1996!) which was a formative game in my childhood, and specifically the fourth level of the first 'episode' in Quake, <a href="https://quake.fandom.com/wiki/E1M4:_the_Grisly_Grotto">E1M4: 'The Grisly Grotto'</a>. I don't know; I liked the name, and so its stuck for the past fourteen years. I can't actually remember why I initially chose it :D</p>
<p>
The first version of GG went live back in 2006. I had always wanted to have a blog, and thought having one was part of what made someone a developer.
More importantly though, while I had learned much of the craft of web development (in ASP.NET Webforms back then), my day job did not afford me the opportunity to 'do it all': design, development, hosting, bug fixing etc. 
So GG was born, a web application of manageable scope that allowed me to fully practice my craft. 
While my skill set is now vastly greater than the meagre needs of this site, it still serves as a periodic 'zen' meditation: rebuilding the same structure over and over again, in the latest JS framework, database architecture or server-side technology that takes my fancy.
</p>
<p>
So far there have been 17 versions, each one distinctive. Earlier versions used ASP.NET Webforms, some used XSLT transforms for markup, some were pure client side javascript and there was a one-off version in pure NodeJS, though most have been partly or entirely .NET code. 
Multiple design frameworks have been used, but mostly I have done the CSS entirely myself for practice. At least six different data storage solutions (databases or otherwise) have been used, and multiple different hosting solutions including Azure cloud for a long time before it ended up here, running a Raspberry PI 3B+ for v16-v17. 
</p>
<p>
I expect there to be many more iterations as I seek to learn new things going forward.
</p>
<h2>About v17 specifically</h2>
<p>
v17 is the first version to be built in the language Go, and a reasonably anaemic use of the language too: aside from a library to use Sqlite, I am using nothing but the packages shipped with the core install (mainly http, database/sql and template/html). The previous two versions of the website were in F# (though in truth largely the same codebase), a language that is one of my favourites for its simplicity and functional style. However, I have really enjoyed building this version in Go.
</p>
<p>
Why use Go? I use it at work, in my infosec job. Its simplicity and the fact it builds to a nice small, fast executable is very useful for the point work I use it for. I decided to see how easy it would be to use it for web sites, and when I discovered <it>how</it> easy it would be, I thought I would have a go at building my standing 'kata' project: this site. I am glad I did.
</p>
<p>
Hosting wise, the site is running on a small <a href="https://www.raspberrypi.org/products/raspberry-pi-3-model-b-plus/">Raspberry PI 3 B+</a>, with no issues :) GG is started via a system service, sits behind a reverse proxy (nginx), and enforces HTTPS via a <a href="https://letsencrypt.org/">LetsEncrypt</a> certificate. Very nice.
</p>
<p>
Cheers.
</p>
{{ end }}{{define "master"}}
<!DOCTYPE html>
<html>
    <head>
        <title>The Grisly Grotto - Deviant Minds Think Alike</title>
        <link href="/static/site.css" rel="stylesheet" />
        <meta charset="utf-8" />
        <meta name="description" content="Personal blog site of Chris Pritchard and Peter Coleman" />
        <meta name="viewport" content="width=device-width, initial-scale=1">
        <meta name="application-name" content="The Grisly Grotto" />
        <link rel="icon" type="image/png" href="/static/favicon.png" />
    </head>
    <body>
        <header>
            <a href="/"><h1 class="site-title">The Grisly Grotto</h1></a>
        </header>
        <nav>
            {{ if loggedIn }}
                <a href="/editor">New Post</a>
            {{ else }}
                <a href="/login">Login</a>
            {{ end }}
            <span>&nbsp;|&nbsp;</span>
            <a href="/archives">Archives</a>
            <span>&nbsp;|&nbsp;</span>
            <a href="/search">Search</a>
            <span>&nbsp;|&nbsp;</span>
            <a href="/about">About</a>
        </nav>
        {{template "content" .}}
        <footer>
            Grisly Grotto v17.0. Site designed and coded by Christopher Pritchard, 2020
        </footer>
        <script src="/static/site.js"></script>
    </body>
</html>
{{end}}`
	templateContent["archives.html"] = `{{ define "content" }}
<h2>Archives</h2>
<ul>
    {{ range .Years }}
        <li>
            <h3>{{ .Year }}</h3>
            <ul>
                {{ range .Months }}
                    <li>
                        <a href="/month/{{ .Month }}/{{ .Year }}">{{ .Month }} ({{ .Count }})</a>
                    </li>
                {{ end }}
            </ul>
        </li>
    {{ end }}
</ul>
<h2>Stories</h2>
<ul>
    {{ range .Stories }}
    <li>
        <a href="/post/{{ .Key }}"><h3>{{ .Title }} ({{ .WordCount }} words)</h3></a>
        <span>Posted by {{ .Author }} at {{ formatDate .Date }}</span>
    </li>
    {{ end }}
</ul>
{{ end }}{{define "master"}}
<!DOCTYPE html>
<html>
    <head>
        <title>The Grisly Grotto - Deviant Minds Think Alike</title>
        <link href="/static/site.css" rel="stylesheet" />
        <meta charset="utf-8" />
        <meta name="description" content="Personal blog site of Chris Pritchard and Peter Coleman" />
        <meta name="viewport" content="width=device-width, initial-scale=1">
        <meta name="application-name" content="The Grisly Grotto" />
        <link rel="icon" type="image/png" href="/static/favicon.png" />
    </head>
    <body>
        <header>
            <a href="/"><h1 class="site-title">The Grisly Grotto</h1></a>
        </header>
        <nav>
            {{ if loggedIn }}
                <a href="/editor">New Post</a>
            {{ else }}
                <a href="/login">Login</a>
            {{ end }}
            <span>&nbsp;|&nbsp;</span>
            <a href="/archives">Archives</a>
            <span>&nbsp;|&nbsp;</span>
            <a href="/search">Search</a>
            <span>&nbsp;|&nbsp;</span>
            <a href="/about">About</a>
        </nav>
        {{template "content" .}}
        <footer>
            Grisly Grotto v17.0. Site designed and coded by Christopher Pritchard, 2020
        </footer>
        <script src="/static/site.js"></script>
    </body>
</html>
{{end}}`
	templateContent["editor.html"] = `{{ define "content" }}

<h2>
    {{ if .NewPost }}
        New Post
    {{ else }}
        Edit Post
    {{ end }}
</h2>

<form method="POST">
    <label for="title">Title</label>
    <input type="text" id="title" name="title" value="{{ .Title }}" />
    <label for="content"></label>
    <textarea id="content" name="content" cols="100" rows="35">{{ raw .Content }}</textarea>
    <input type="checkbox" id="isStory" name="isStory" {{ if .IsStory }}checked="checked"{{ end }} />
    <label for="isStory">Is Story</label>
    <input type="submit" value="Submit" />
    <span>{{ .PostError }}</span>
</form>

{{ end }}{{define "master"}}
<!DOCTYPE html>
<html>
    <head>
        <title>The Grisly Grotto - Deviant Minds Think Alike</title>
        <link href="/static/site.css" rel="stylesheet" />
        <meta charset="utf-8" />
        <meta name="description" content="Personal blog site of Chris Pritchard and Peter Coleman" />
        <meta name="viewport" content="width=device-width, initial-scale=1">
        <meta name="application-name" content="The Grisly Grotto" />
        <link rel="icon" type="image/png" href="/static/favicon.png" />
    </head>
    <body>
        <header>
            <a href="/"><h1 class="site-title">The Grisly Grotto</h1></a>
        </header>
        <nav>
            {{ if loggedIn }}
                <a href="/editor">New Post</a>
            {{ else }}
                <a href="/login">Login</a>
            {{ end }}
            <span>&nbsp;|&nbsp;</span>
            <a href="/archives">Archives</a>
            <span>&nbsp;|&nbsp;</span>
            <a href="/search">Search</a>
            <span>&nbsp;|&nbsp;</span>
            <a href="/about">About</a>
        </nav>
        {{template "content" .}}
        <footer>
            Grisly Grotto v17.0. Site designed and coded by Christopher Pritchard, 2020
        </footer>
        <script src="/static/site.js"></script>
    </body>
</html>
{{end}}`
	templateContent["latest.html"] = `{{ define "content" }}
<h2>Latest Posts</h2>
{{ range .Posts }}
    <div>
        <a href="/post/{{ .Key }}"><h3 class="post-title">{{ .Title }}</h3></a>
        <div class="post-summary">Posted by {{ .Author }} at {{ formatDate .Date }}. <a href="/post/{{ .Key }}#comments">Comments ({{ .CommentCount }})</a></div>
        <div class="post-content">
            {{ if .IsStory }}
                <p><a href="/post/{{ .Key }}">Click through to read ({{ .WordCount }} words)...</a></p>
            {{ else }}
                {{ raw .Content }}
            {{ end }}
        </div>
    </div>
{{ end }}
<div>
    {{ if .NotFirstPage }}
        <form method="GET" class="prev-link">
            <input type="hidden" name="page" value="{{ .PrevPage }}" />
            <input type="submit" value="Previous Page" />
        </form>
    {{ end }}
    <form method="GET" class="next-link">
        <input type="hidden" name="page" value="{{ .NextPage }}" />
        <input type="submit" value="Next Page" />
    </form>
</div>
{{ end }}{{define "master"}}
<!DOCTYPE html>
<html>
    <head>
        <title>The Grisly Grotto - Deviant Minds Think Alike</title>
        <link href="/static/site.css" rel="stylesheet" />
        <meta charset="utf-8" />
        <meta name="description" content="Personal blog site of Chris Pritchard and Peter Coleman" />
        <meta name="viewport" content="width=device-width, initial-scale=1">
        <meta name="application-name" content="The Grisly Grotto" />
        <link rel="icon" type="image/png" href="/static/favicon.png" />
    </head>
    <body>
        <header>
            <a href="/"><h1 class="site-title">The Grisly Grotto</h1></a>
        </header>
        <nav>
            {{ if loggedIn }}
                <a href="/editor">New Post</a>
            {{ else }}
                <a href="/login">Login</a>
            {{ end }}
            <span>&nbsp;|&nbsp;</span>
            <a href="/archives">Archives</a>
            <span>&nbsp;|&nbsp;</span>
            <a href="/search">Search</a>
            <span>&nbsp;|&nbsp;</span>
            <a href="/about">About</a>
        </nav>
        {{template "content" .}}
        <footer>
            Grisly Grotto v17.0. Site designed and coded by Christopher Pritchard, 2020
        </footer>
        <script src="/static/site.js"></script>
    </body>
</html>
{{end}}`
	templateContent["login.html"] = `{{ define "content" }}
    <h2>Login</h2>
    <form method="POST">
        <div>
            <label for="username">Username</label>
            <input id="username" name="username" type="text" />
        </div>
        <div>
            <label for="password">Password</label>
            <input id="password" name="password" type="password" />
        </div>
        <div>
            <input type="submit" value="Submit">
            <span>{{ .Error }}</span>
        </div>
    </form>
{{ end }}{{define "master"}}
<!DOCTYPE html>
<html>
    <head>
        <title>The Grisly Grotto - Deviant Minds Think Alike</title>
        <link href="/static/site.css" rel="stylesheet" />
        <meta charset="utf-8" />
        <meta name="description" content="Personal blog site of Chris Pritchard and Peter Coleman" />
        <meta name="viewport" content="width=device-width, initial-scale=1">
        <meta name="application-name" content="The Grisly Grotto" />
        <link rel="icon" type="image/png" href="/static/favicon.png" />
    </head>
    <body>
        <header>
            <a href="/"><h1 class="site-title">The Grisly Grotto</h1></a>
        </header>
        <nav>
            {{ if loggedIn }}
                <a href="/editor">New Post</a>
            {{ else }}
                <a href="/login">Login</a>
            {{ end }}
            <span>&nbsp;|&nbsp;</span>
            <a href="/archives">Archives</a>
            <span>&nbsp;|&nbsp;</span>
            <a href="/search">Search</a>
            <span>&nbsp;|&nbsp;</span>
            <a href="/about">About</a>
        </nav>
        {{template "content" .}}
        <footer>
            Grisly Grotto v17.0. Site designed and coded by Christopher Pritchard, 2020
        </footer>
        <script src="/static/site.js"></script>
    </body>
</html>
{{end}}`
	templateContent["month.html"] = `{{ define "content" }}
<h2>{{ .Month }}, {{ .Year }}</h2>
{{ range .Posts }}
    <div>
        <a href="/post/{{ .Key }}"><h3 class="post-title">{{.Title}}</h3></a>
        <div class="post-summary">Posted by {{ .Author }} at {{ formatDate .Date }}. <a href="/post/{{ .Key }}#comments">Comments ({{ .CommentCount }})</a></div>
        <div class="post-content">
        <div>
            {{ if .IsStory }}
                <p><a href="/post/{{ .Key }}">Click through to read ({{ .WordCount }} words)...</a></p>
            {{ else }}
                {{ raw .Content }}
            {{ end }}
        </div>
    </div>
{{ end }}
<div>
    <form method="GET" action="/month/{{ .PrevMonth }}/{{ .PrevYear }}" class="prev-link">
        <input type="submit" value="{{ .PrevMonth }}, {{ .PrevYear }}" />
    </form>
    <form method="GET" action="/month/{{ .NextMonth }}/{{ .NextYear }}" class="next-link">
        <input type="submit" value="{{ .NextMonth }}, {{ .NextYear }}" />
    </form>
</div>
{{ end }}{{define "master"}}
<!DOCTYPE html>
<html>
    <head>
        <title>The Grisly Grotto - Deviant Minds Think Alike</title>
        <link href="/static/site.css" rel="stylesheet" />
        <meta charset="utf-8" />
        <meta name="description" content="Personal blog site of Chris Pritchard and Peter Coleman" />
        <meta name="viewport" content="width=device-width, initial-scale=1">
        <meta name="application-name" content="The Grisly Grotto" />
        <link rel="icon" type="image/png" href="/static/favicon.png" />
    </head>
    <body>
        <header>
            <a href="/"><h1 class="site-title">The Grisly Grotto</h1></a>
        </header>
        <nav>
            {{ if loggedIn }}
                <a href="/editor">New Post</a>
            {{ else }}
                <a href="/login">Login</a>
            {{ end }}
            <span>&nbsp;|&nbsp;</span>
            <a href="/archives">Archives</a>
            <span>&nbsp;|&nbsp;</span>
            <a href="/search">Search</a>
            <span>&nbsp;|&nbsp;</span>
            <a href="/about">About</a>
        </nav>
        {{template "content" .}}
        <footer>
            Grisly Grotto v17.0. Site designed and coded by Christopher Pritchard, 2020
        </footer>
        <script src="/static/site.js"></script>
    </body>
</html>
{{end}}`
	templateContent["search.html"] = `{{ define "content" }}
<form method="GET">
    <input type="text" name="searchTerm" />
    <input type="submit" value="Search" />
</form>
{{ if .SearchTerm }}
{{ if .ZeroResults }}
    <div>No results found for '{{ .SearchTerm }}'</div>
{{ else }}
    <h3>Results for '{{ .SearchTerm }}'</h3>
    {{ range .Results }}
        <div>
            <a href="/post/{{ .Key }}"><h4>{{ .Title }}</h4></a>
            <div>{{ .Content }}</div>
            <span>Posted by {{ .Author }} at {{ formatDate .Date }}</span>
        </div>
    {{ end }}
{{ end }}
{{ end }}
{{ end }}{{define "master"}}
<!DOCTYPE html>
<html>
    <head>
        <title>The Grisly Grotto - Deviant Minds Think Alike</title>
        <link href="/static/site.css" rel="stylesheet" />
        <meta charset="utf-8" />
        <meta name="description" content="Personal blog site of Chris Pritchard and Peter Coleman" />
        <meta name="viewport" content="width=device-width, initial-scale=1">
        <meta name="application-name" content="The Grisly Grotto" />
        <link rel="icon" type="image/png" href="/static/favicon.png" />
    </head>
    <body>
        <header>
            <a href="/"><h1 class="site-title">The Grisly Grotto</h1></a>
        </header>
        <nav>
            {{ if loggedIn }}
                <a href="/editor">New Post</a>
            {{ else }}
                <a href="/login">Login</a>
            {{ end }}
            <span>&nbsp;|&nbsp;</span>
            <a href="/archives">Archives</a>
            <span>&nbsp;|&nbsp;</span>
            <a href="/search">Search</a>
            <span>&nbsp;|&nbsp;</span>
            <a href="/about">About</a>
        </nav>
        {{template "content" .}}
        <footer>
            Grisly Grotto v17.0. Site designed and coded by Christopher Pritchard, 2020
        </footer>
        <script src="/static/site.js"></script>
    </body>
</html>
{{end}}`
	templateContent["single.html"] = `{{ define "content" }}
<h2>{{ .Post.Title }}</h2>
<span>Posted by {{ .Post.Author }} at {{ formatDate .Post.Date }}.</span>
<div>
    {{ raw .Post.Content }}
</div>
<div>
    {{ if .OwnBlog }}
        <form method="GET" action="/editor/{{ .Post.Key }}">
            <input type="submit" value="Edit" />
        </form>
        <form method="POST" action="/delete-post/{{ .Post.Key }}">
            <input type="submit" value="Delete" class="confirm-click" />
        </form>
    {{ end }}
</div>
<h2 id="comments">Comments</h2>
{{ range .Post.Comments }}
<div>
    <div class="post-summary">Commented by {{ .Author }} at {{ formatDate .Date }}:</div>
    <div class="post-content">
        {{ raw .Content }}
    </div>
    {{ if $.OwnBlog }}
        <form method="POST" action="/delete-comment/{{ .ID }}?postKey={{ $.Post.Key }}">
            <input type="submit" value="Delete" class="confirm-click" />
        </form>
    {{ end }}
</div>
{{ end }}
{{ if .CanComment }}
<form method="POST">
    <label for="author">Author</label>
    <input type="text" id="author" name="author">
    <label for="content">Content</label>
    <textarea rows="3" cols="50" id="content" name="content"></textarea>
    <input type="submit" value="Comment">
    <span>{{ .CommentError }}</span>
</form>
{{ end }}
{{ end }}{{define "master"}}
<!DOCTYPE html>
<html>
    <head>
        <title>The Grisly Grotto - Deviant Minds Think Alike</title>
        <link href="/static/site.css" rel="stylesheet" />
        <meta charset="utf-8" />
        <meta name="description" content="Personal blog site of Chris Pritchard and Peter Coleman" />
        <meta name="viewport" content="width=device-width, initial-scale=1">
        <meta name="application-name" content="The Grisly Grotto" />
        <link rel="icon" type="image/png" href="/static/favicon.png" />
    </head>
    <body>
        <header>
            <a href="/"><h1 class="site-title">The Grisly Grotto</h1></a>
        </header>
        <nav>
            {{ if loggedIn }}
                <a href="/editor">New Post</a>
            {{ else }}
                <a href="/login">Login</a>
            {{ end }}
            <span>&nbsp;|&nbsp;</span>
            <a href="/archives">Archives</a>
            <span>&nbsp;|&nbsp;</span>
            <a href="/search">Search</a>
            <span>&nbsp;|&nbsp;</span>
            <a href="/about">About</a>
        </nav>
        {{template "content" .}}
        <footer>
            Grisly Grotto v17.0. Site designed and coded by Christopher Pritchard, 2020
        </footer>
        <script src="/static/site.js"></script>
    </body>
</html>
{{end}}`
}
