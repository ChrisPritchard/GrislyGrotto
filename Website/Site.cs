using System;
using System.Web;
using System.Web.Configuration;
using System.Web.SessionState;
using System.Linq;
using System.Xml.Linq;
using System.Collections;
using System.Collections.Generic;

public class Site
{
    private HttpRequest Request;
    private HttpSessionState Session;

    XElement pageNode;
    string currentAuthor;
    XDocument authorsXml;
    XDocument linksXml;
    XDocument blogsXml;
    XDocument commentsXml;
    XDocument quotesXml;

    /// <summary>
    /// This constructor initializes the one time Site object with the context parameters
    /// </summary>
    /// <param name="Request">The request object of the current context</param>
    /// <param name="Session">The current session</param>
    public Site(HttpRequest Request, HttpSessionState Session)
    {
        this.Request = Request;
        this.Session = Session;
    }

    /// <summary>
    /// This function should generate the Xml Data to be transformed onto the page
    /// </summary>
    /// <returns>An XDocument object representing the Xml Document</returns>
    public XDocument GetXml()
    {
        XDocument document = new XDocument(new XDeclaration("1.0", "UTF-8", "yes"));
        pageNode = new XElement("page", new XAttribute("title", "The Grisly Grotto - Deviant Minds Think Alike"));
 
        authorsXml = XDocument.Load(Request.PhysicalApplicationPath + WebConfigurationManager.AppSettings["authorsXmlPath"]);
        linksXml = XDocument.Load(Request.PhysicalApplicationPath + WebConfigurationManager.AppSettings["linksXmlPath"]);
        blogsXml = XDocument.Load(Request.PhysicalApplicationPath + WebConfigurationManager.AppSettings["blogsXmlPath"]);
        commentsXml = XDocument.Load(Request.PhysicalApplicationPath + WebConfigurationManager.AppSettings["commentsXmlPath"]);
        quotesXml = XDocument.Load(Request.PhysicalApplicationPath + WebConfigurationManager.AppSettings["quotesXmlPath"]);        

        //Find the current page's author
        if (CheckLogin())
            currentAuthor = (string)Session["loggeduser"];
        else if (Request.QueryString["author"] != null && LinqAuthorExists(Request.QueryString["author"]))
            currentAuthor = Request.QueryString["author"];
        else
            currentAuthor = DefaultAuthor();
        AddAuthorDetails();

        string targetblog = CheckBlogPost();
        if(targetblog == null)
            targetblog = CheckCommentPost();
        CheckLinksEdit();

        //Sidebar
        if (Request.Form["editor"] == null && (targetblog != null || Request.Form["blogpost"] == null))
        {
            //only display the side bar if the editor is not shown
            AddUserBox();
            AddContents();
            AddUserLinks(); 
            AddUserRecent();
            AddUserHistory();
        }

        //Main content
        AddRandomImage();
        AddQuote();
        if (targetblog != null)
            AddSingleBlog(targetblog);
        else
        {
            if (Request.Form["editor"] != null || (targetblog == null && Request.Form["blogpost"] != null))
                AddEditor(Request.Form["hiddentitle"]);
            else if (Request.QueryString["blog"] != null)
                AddSingleBlog(Request.QueryString["blog"]);
            else if (Request.QueryString["month"] != null)
                AddMonthsBlogs(Request.QueryString["month"]);
            else
                AddRecentBlogs();
        }

        document.Add(pageNode);
        return document;
    }

    #region Preliminary checks

    private bool CheckLogin()
    {
        if (Request.Form["login"] != null)
        {
            List<string> validatedName = LinqAuthorByCredentials(Request.Form["username"], Request.Form["password"]);
            if (validatedName.Count() != 0)
            {
                Session["loggeduser"] = validatedName.Single();
                return true;
            }
            else
                pageNode.Add(new XElement("error", new XAttribute("area", "login")));
        }
        else if (Request.Form["logout"] != null)
            Session["loggeduser"] = null;
        return false;
    }

    private void AddAuthorDetails()
    {
        if(Session["loggeduser"] != null)
            pageNode.Add(new XElement("author", new XAttribute("name", currentAuthor), new XAttribute("logged", (string)Session["loggeduser"])));
        else
            pageNode.Add(new XElement("author", new XAttribute("name", currentAuthor)));
    }

    private string DefaultAuthor()
    {
        List<string> primaryAuthor = LinqPrimaryAuthor();
        if (primaryAuthor.Count() > 0)
            return primaryAuthor[0];
        else
            return string.Empty;
    }

    private string CheckBlogPost()
    {
        if (Request.Form["blogpost"] != null && Session["loggeduser"] != null)
        {
            if(Request.Form["blogtitle"].Length == 0)
                pageNode.Add(new XElement("error", new XAttribute("area", "blogtitle")));
            else if (LinqSingleBlog(Request.Form["blogtitle"]).Count() > 0 && Request.Form["hiddentitle"] != Request.Form["blogtitle"])
                pageNode.Add(new XElement("error", new XAttribute("area", "uniquetitle")));
            if(Request.Form["blogtext"].Length == 0)
                pageNode.Add(new XElement("error", new XAttribute("area", "blogtext")));
            if (Request.Form["blogtitle"].Length > 0 && Request.Form["blogtext"].Length > 0 && (LinqSingleBlog(Request.Form["blogtitle"]).Count() == 0 || Request.Form["hiddentitle"] == Request.Form["blogtitle"]))
            {
                XElement newBlog = new XElement("blog",
                    new XAttribute("title", Request.Form["blogtitle"]),
                    new XAttribute("author", (string)Session["loggeduser"]),
                    new XAttribute("date", DateTime.Now.ToUniversalTime()),
                    new XText(Request.Form["blogtext"]));

                if (Request.Form["hiddentitle"] != null)
                {
                    List<XElement> oldBlog = LinqSingleBlog(Request.Form["hiddentitle"]);
                    if (oldBlog.Count == 0)
                        blogsXml.Elements("blogs").Single().Add(newBlog);
                    else
                    {
                        newBlog.SetAttributeValue("date", oldBlog[0].Attribute("date").Value);
                        LinqUpdateBlog(Request.Form["hiddentitle"], newBlog);
                    }
                }
                else
                    blogsXml.Elements("blogs").Single().Add(newBlog);

                blogsXml.Save(Request.PhysicalApplicationPath + WebConfigurationManager.AppSettings["blogsXmlPath"]);
                return Request.Form["blogtitle"];
            }
        }
        else if (Request.Form["blogpost"] != null)
            pageNode.Add(new XElement("error", new XAttribute("area", "session")));
        return null;
    }

    private string CheckCommentPost()
    {
        if (Request.Form["commentpost"] != null)
        {
            if (Request.Form["commentauthor"].Length == 0)
                pageNode.Add(new XElement("error", new XAttribute("area", "commentauthor")));
            if (Request.Form["commenttext"].Length == 0)
                pageNode.Add(new XElement("error", new XAttribute("area", "commenttext")));
            if (Request.Form["commentauthor"].Length > 0 && Request.Form["commenttext"].Length > 0)
            {
                XElement newComment = new XElement("comment",
                new XAttribute("blog", Request.Form["commentblog"]),
                new XAttribute("author", Request.Form["commentauthor"]),
                new XAttribute("date", DateTime.Now.ToUniversalTime()),
                new XText(Request.Form["commenttext"]));

                commentsXml.Elements("comments").Single().Add(newComment);

                commentsXml.Save(Request.PhysicalApplicationPath + WebConfigurationManager.AppSettings["commentsXmlPath"]);
            }

            return Request.Form["commentblog"];
        }
        return null;
    }

    private void CheckLinksEdit()
    {
        if (Session["loggeduser"] != null)
        {
            if (Request.Form["deletecategory"] != null)
                LinqDeleteLinkCategory(Request.Form["categorytitle"]);
            else if (Request.Form["deletelink"] != null)
                LinqDeleteLink(Request.Form["linktext"]);
            else if (Request.Form["addcategory"] != null)
                LinqAddLinkCategory(Request.Form["categorytitle"]);
            else if (Request.Form["addlink"] != null)
                LinqAddLink(Request.Form["hiddencategory"], Request.Form["linktext"], Request.Form["linkhref"]);
        }
        else if (Request.Form["deletecategory"] != null || Request.Form["deletelink"] != null || Request.Form["addcategory"] != null || Request.Form["addlink"] != null)
            pageNode.Add(new XElement("error", new XAttribute("area", "session")));
    }

    #endregion

    #region Sidebar

    private void AddRandomImage()
    {
        string[] paths = System.IO.Directory.GetFiles(Request.PhysicalApplicationPath + "Images\\Random\\");
        string path = paths[new Random().Next(0, paths.Length)].Substring(Request.PhysicalApplicationPath.Length).Replace("\\","/");
        pageNode.Add(new XElement("randomimage", new XAttribute("path", path)));
    }

    private void AddUserBox()
    {
        pageNode.Add(new XElement("userbox"));
    }

    private void AddContents()
    {
        List<string> authors = LinqAuthorList();

        XElement authorlinks = new XElement("authorlinks");
        foreach (string author in authors)
            authorlinks.Add(new XElement("authorlink", new XAttribute("name", author)));

        pageNode.Add(authorlinks);
    }

    private void AddUserLinks()
    {
        List<XElement> categories = LinqLinks();

        XElement links = new XElement("links");
        foreach (XElement category in categories)
            links.Add(category);

        pageNode.Add(links);
    }

    private void AddUserRecent()
    {
        XElement recentblogs = new XElement("recentblogs");
        
        List<string> recentblogsList = LinqRecentBlogTitles(10);
        foreach (string recentblog in recentblogsList)
            recentblogs.Add(new XElement("recentblog", new XAttribute("title", recentblog)));

        pageNode.Add(recentblogs);
    }

    private void AddUserHistory()
    {
        XElement history = new XElement("history");

        List<string> monthsList = LinqMonthList();
        foreach (string month in monthsList)
        {
            int count = LinqBlogCountOfMonth(month);

            XElement specificMonth = new XElement("month",
                new XAttribute("text", string.Format("{0} ({1} Posts)", month, count)), new XAttribute("link", DateTime.Parse(month).ToString("MM/yyyy")));
            history.Add(specificMonth);
        }

        pageNode.Add(history);
    }

    #endregion

    #region Main content

    private void AddQuote()
    {
        XElement quote = LinqRandomQuote();
        pageNode.Add(quote);
    }

    private void AddEditor(string blog)
    {
        XElement editorNode = new XElement("editor");
        if (blog != null)
        {
            List<XElement> oldBlog = LinqSingleBlog(blog);
            if (oldBlog.Count == 0)
            {
                pageNode.Add(new XElement("error", new XAttribute("area", "oldblog")));
                return;
            }
            else
                editorNode.Add(oldBlog[0]);
        }
        else if(Request.Form["blogpost"] != null)
            editorNode.Add(new XElement("blog", new XAttribute("title", Request.Form["blogtitle"]), new XText(Request.Form["blogtext"])));
        pageNode.Add(editorNode);
    }

    private void AddSingleBlog(string blog)
    {
        List<XElement> singleBlog = LinqSingleBlog(blog);
        if (singleBlog.Count() == 0)
            pageNode.Add(new XElement("error", new XAttribute("area", "blognotfound")));
        else
        {
            AppendBlogList(singleBlog, false);
            List<XElement> commentsResults = LinqCommentsForBlog(blog);
            
            XElement comments = new XElement("comments");
            foreach (XElement comment in commentsResults)
            {
                DateTime postedDate = DateTime.Parse(comment.Attribute("date").Value);
                comment.SetAttributeValue("date", postedDate.ToString("dddd, dd/MM/yyyy, h:mm tt"));

                comments.Add(comment);
            }
            pageNode.Add(comments);

            if (Request.Form["commentpost"] != null && (Request.Form["commentauthor"].Length == 0 || Request.Form["commenttext"].Length == 0))
                pageNode.Add(new XElement("commenteditor", new XAttribute("author", Request.Form["commentauthor"]), new XText(Request.Form["commenttext"])));
        }
    }

    private void AddMonthsBlogs(string month)
    {
        List<XElement> blogResults = LinqBlogsFromMonth(month);
        AppendBlogList(blogResults, true);
    }

    private void AddRecentBlogs()
    {
        List<XElement> blogResults = LinqRecentBlogs(5);
        AppendBlogList(blogResults, true);
    }

    private void AppendBlogList(List<XElement> blogList, bool addCommentCount)
    {
        XElement blogs = new XElement("blogs");
        foreach (XElement blog in blogList)
        {
            DateTime postedDate = DateTime.Parse(blog.Attribute("date").Value);
            if (postedDate.ToString("dd MM yyyy") == DateTime.Now.ToString("dd MM yyyy"))
                blog.SetAttributeValue("date", "Today, " + postedDate.ToString("h:mm tt"));
            else if (postedDate.ToString("dd MM yyyy") == DateTime.Now.AddDays(-1).ToString("dd MM yyyy"))
                blog.SetAttributeValue("date", "Yesterday, " + postedDate.ToString("h:mm tt"));
            else
                blog.SetAttributeValue("date", postedDate.ToString("dddd, dd/MM/yyyy, h:mm tt"));

            if (addCommentCount)
            {
                int commentCount = LinqCommentCount(blog.Attribute("title").Value);
                blog.Add(new XAttribute("comments", commentCount.ToString()));
            }

            blogs.Add(blog);
        }
        pageNode.Add(blogs);
    }

    #endregion

    #region Linq

    private List<string> LinqAuthorByCredentials(string username, string password)
    {
        return authorsXml.Element("authors").Descendants().Where(author => author.Attribute("username").Value == username
            && author.Attribute("password").Value == password).Select(author => author.Attribute("name").Value).ToList();
    }

    private bool LinqAuthorExists(string name)
    {
        return authorsXml.Element("authors").Descendants().Where(author => author.Attribute("name").Value == name).ToList().Count > 0;
    }

    private List<string> LinqPrimaryAuthor()
    {
        return authorsXml.Descendants().Where(author => author.Attribute("primary") != null).
             Select(author => author.Attribute("name").Value).ToList();
    }

    private void LinqUpdateBlog(string oldtitle, XElement newBlog)
    {
        blogsXml.Elements("blogs").Elements().
            Where(blog => blog.Attribute("title").Value == oldtitle).Single().ReplaceWith(newBlog);
    }

    private void LinqDeleteLinkCategory(string categorytitle)
    {
        linksXml.Element("categories").Elements().
            Where(element => element.Attribute("author").Value == (string)Session["loggeduser"] &&
                element.Attribute("title").Value == categorytitle).Remove();
        linksXml.Save(Request.PhysicalApplicationPath + WebConfigurationManager.AppSettings["linksXmlPath"]);
    }

    private void LinqDeleteLink(string linktext)
    {
        linksXml.Element("categories").Elements().
            Where(element => element.Attribute("author").Value == (string)Session["loggeduser"]).Elements().
            Where(element => element.Attribute("text").Value == linktext).Remove();
        linksXml.Save(Request.PhysicalApplicationPath + WebConfigurationManager.AppSettings["linksXmlPath"]);
    }

    private void LinqAddLinkCategory(string title)
    {
        linksXml.Element("categories").Add(new XElement("category",
            new XAttribute("author", (string)Session["loggeduser"]),
            new XAttribute("title", title)));
        linksXml.Save(Request.PhysicalApplicationPath + WebConfigurationManager.AppSettings["linksXmlPath"]);
    }

    private void LinqAddLink(string category, string text, string href)
    {
        linksXml.Element("categories").Elements().
             Where(element => element.Attribute("title").Value == category &&
                 element.Attribute("author").Value == (string)Session["loggeduser"]).Single().
                 Add(new XElement("link", new XAttribute("text", text), new XAttribute("href", href)));
        linksXml.Save(Request.PhysicalApplicationPath + WebConfigurationManager.AppSettings["linksXmlPath"]);
    }

    private List<string> LinqAuthorList()
    {
        return blogsXml.Element("blogs").Elements("blog").OrderByDescending(author => author.Attribute("date").Value).
             Select(author => author.Attribute("author").Value).Distinct().ToList();
    }

    private List<XElement> LinqLinks()
    {
        return linksXml.Element("categories").Elements().
             Where(category => category.Attribute("author").Value == currentAuthor).ToList();
    }

    private List<string> LinqRecentBlogTitles(int p)
    {
        return blogsXml.Element("blogs").Elements("blog").
            Where(blog => blog.Attribute("author").Value == currentAuthor).
            OrderByDescending(blog => DateTime.Parse(blog.Attribute("date").Value)).
            Select(blog => blog.Attribute("title").Value).Take(10).ToList();
    }

    private List<string> LinqMonthList()
    {
        return blogsXml.Element("blogs").Elements("blog").
             Where(blog => blog.Attribute("author").Value == currentAuthor).
             OrderByDescending(blog => DateTime.Parse(blog.Attribute("date").Value)).
             Select(blog => DateTime.Parse(blog.Attribute("date").Value).ToString("MMMM yyyy")).Distinct().ToList();
    }

    private int LinqBlogCountOfMonth(string month)
    {
        return blogsXml.Element("blogs").Elements("blog").
             Where(blog => DateTime.Parse(blog.Attribute("date").Value).ToString("MMMM yyyy") == month &&
                 blog.Attribute("author").Value == currentAuthor).ToList().Count();
    }

    private XElement LinqRandomQuote()
    {
        return quotesXml.Element("quotes").Elements().ToList()[new Random().Next(0, quotesXml.Element("quotes").Elements().Count())];
    }

    private List<XElement> LinqSingleBlog(string blog)
    {
        return blogsXml.Element("blogs").Elements().Where(singleblog => singleblog.Attribute("title").Value == blog).ToList();
    }

    private List<XElement> LinqCommentsForBlog(string blog)
    {
        return commentsXml.Element("comments").Elements().
            Where(comment => comment.Attribute("blog").Value == blog).
            OrderBy(comment => DateTime.Parse(comment.Attribute("date").Value)).ToList();
    }

    private List<XElement> LinqBlogsFromMonth(string month)
    {
        return blogsXml.Element("blogs").Elements().
            Where(blog => DateTime.Parse(blog.Attribute("date").Value).ToString("MM/yyyy") == month).
            Where(blog => blog.Attribute("author").Value == currentAuthor).
            OrderByDescending(blog => DateTime.Parse(blog.Attribute("date").Value)).
            Select(blog => blog).ToList();
    }

    private List<XElement> LinqRecentBlogs(int count)
    {
        return blogsXml.Element("blogs").Elements().
            Where(blog => blog.Attribute("author").Value == currentAuthor).
            OrderByDescending(blog => DateTime.Parse(blog.Attribute("date").Value)).
            Take(count).ToList();
    }

    private int LinqCommentCount(string blog)
    {
        return commentsXml.Element("comments").Elements().
            Where(comment => comment.Attribute("blog").Value == blog).ToList().Count();
    }

    #endregion

}
