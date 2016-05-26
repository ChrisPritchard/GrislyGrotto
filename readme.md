## Release 13

<b>Release Date:</b> 24th April 2015

<b>Release Post:</b> <a href="http://grislygrotto.azurewebsites.net/p/gg-135">GG 13.5</a>

<b>Frontend:</b> ASP.NET MVC5, .NET 4.5 and C#6

<b>Backend:</b> SQL Server CE

<b>Dev Tool:</b> Visual Studio 2015

The main change from 13 was the switch off from Azure Search to SQL Server CE. I'd been using CE at work, and Search was causing a lot of problems. The backup solution is worse with the current model, at the time of writing, but the return of functionality like archives and stories is welcome. There were also a lot of UI changes I was able to make as part of this - like back and forth on posts. I also brightened the UI up from Black and Gold, though kept the animation. In the code base, taking advantage of C# 6's availability, I reworked the code to be less orthodox but more intuitive. Accordingly, the site is the lightest MVC version its ever been.