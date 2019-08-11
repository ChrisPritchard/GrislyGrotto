My name is Christopher Pritchard, and I work as a senior-level software developer and architect in Wellington, New Zealand. 
I operate through my company, [Aquinas IT](https://aquinas.nz), of which I am the director. My professional goals over the next few years include trying to grow this business.
In the past, I have worked for long periods as a full-time employee (largely at the IT vendor Provoke Solutions).

I have been working in the IT industry for over fifteen years now, primary in development or development-related roles. While management, solution design and technical leadership have increasingly been part of my responsibilities, I always prefer to stay close to the code. I think I'm pretty good at what I do :D

Outside of work, my hobbies are writing (successive participant/winner in the NaNoWriMo competition each year I'm proud to say), and gaming. I also do a lot of personal projects, such as the one you are looking at now.

Finally, and by no means least, I am a happily married man with a young daughter, as well as a small dog.

## Contact

I can be reached most easily by my personal email address: [chrispritchard.nz@gmail.com](mailto:chrispritchard.nz@gmail.com).<br/>
Company queries can be made via my company address: [contact@aquinas.nz](mailto:contact@aquinas.nz).<br/>
My LinkedIn profile is [here](https://nz.linkedin.com/pub/christopher-pritchard/a/9b6/a66).<br/>
My Github profile is [here](https://github.com/ChrisPritchard).

## About the Grisly Grotto

The first version of GG went live back in 2006. I had always wanted to have a blog, and thought having one was part of what made someone a developer.
More importantly though, while I had learned much of the craft of web development (in ASP.NET Webforms back then), my day job did not afford me the opportunity to 'do it all': design, development, hosting, bug fixing etc. 
So GG was born, a web application of manageable scope that allowed me to fully practice my craft. 
While my skill set is now vastly greater than the meagre needs of this site, it still serves as a periodic 'zen' meditation: rebuilding the same structure over and over again, in the latest JS framework, database architecture or server-side technology that takes my fancy.

So far there have been 16 versions, each one distinctive. Earlier versions used ASP.NET Webforms, some used XSLT transforms for markup, some were pure client side javascript (including a one-off version in pure NodeJS). 
Multiple design frameworks have been used, although mostly I have done the CSS entirely myself for practice. At least six different data storage solutions (databases or otherwise) have been used, and multiple different hosting solutions before it ended up here, on Azure for v11+. 
I expect there to be many more iterations as I seek to learn new things going forward."

## About v16 specifically

v16 is the second version to be built entirely in F#, using the [Giraffe](https://github.com/giraffe-fsharp/Giraffe) framework and ASPNET.Core. It is the first version in a while to not be hosted on Azure - instead I am hosting it on my own server, with my own purchased domain (which I haven't done for over a decade). Its database backend has been switched from SQL Server to [SQLite](https://www.sqlite.org/index.html), something that was very easy given I had the database content backed up to serialised JSON, and both SQL Server and SQLite can be accessed identically using Entity Framework Core. I just had to change the connection string and nuget packages, then do a small migration and I was done!

v16 additionally integrates markdown for some page content (including this page) and makes a number of other small improvements. In the time between v15 and v16 my F# knowledge expanded exponentially - I even won the honour of being recognised as an 'F# Expert' for [one of my side projects](https://github.com/ChrisPritchard/FSH) - so that is being reflected in this build.

Hosting wise, the site at present is running on a small [Raspberry PI 3 B+](https://www.raspberrypi.org/products/raspberry-pi-3-model-b-plus/), with no issues :) With the shift to Linux hosting came an opportunity to do/learn some things I didn't get exposure to in Azure: GG is now started via a system service, sits behind a reverse proxy, and enforces HTTPS via a [LetsEncrypt](https://letsencrypt.org/) certificate. Very nice.

Cheers.
