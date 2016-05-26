## Release 5.0  

<b>Release Date:</b> 19th January, 2009

<b>Release Post:</b> <a href="http://grislygrotto.azurewebsites.net/p/gg5">GG5</a>

<b>Frontend:</b> ASP.NET MVC 1.0 with a custom XSL View Engine, .NET 3.5

<b>Backend:</b> Linq to SQL

<b>Dev Tool:</b> Visual Studio 2008

The first MVC version of the site, built with the first version of ASP.NET MVC. I wasn't yet ready to leave XSLT behind though (and likely was still using it heavily at work) and so I tested MVC's extensibility by replacing its built in view engine (which was pretty bad, being pre-razor) with one that transformed XML with XSLT. Also switched off XML data files (likely because at this point it would have been painfully slow due to their growing size) to SQL, accessed via the now-dead but awesome-at-the-time Linq to SQL framework.