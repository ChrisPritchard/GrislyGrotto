using System;
using System.Web;
using System.Web.Configuration;
using System.Web.SessionState;
using System.Linq;
using System.Xml;
using System.Xml.Xsl;
using System.Xml.Linq;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace GrislyGrotto
{
	public class Blog
	{
	    private HttpRequest Request;
	    private HttpSessionState Session;

	    XElement xPageNode;
		string sCurrentAuthor;

        string sPhysicalApplicationPath;

	    XDocument xUsersXml;
	    XDocument xBlogsXml;
	    XDocument xQuotesXml;

        /// <summary>
        /// This method generates the html content of this module, and writes it through the provided test writer
        /// </summary>
        /// <param name="twTextWriter"></param>
		public void TransformOntoStream(TextWriter twTextWriter)
		{
            sPhysicalApplicationPath = Request.PhysicalApplicationPath;

			XslCompiledTransform xTransform = new XslCompiledTransform();
	        xTransform.Load(Request.PhysicalApplicationPath + "blog/Xslt.xslt");
            xTransform.Transform(GetXmlReader(), null, twTextWriter);
		}
		
	    /// <summary>
	    /// This constructor initializes the one time Site object with the context parameters
	    /// </summary>
	    /// <param name="Request">The request object of the current context</param>
	    /// <param name="Session">The current session</param>
	    public Blog(HttpRequest Request, HttpSessionState Session)
	    {
	        this.Request = Request;
	        this.Session = Session;
	    }

	    /// <summary>
	    /// This function should generate the Xml Data to be transformed onto the page
	    /// </summary>
	    /// <returns>An XDocument object representing the Xml Document</returns>
	    private XmlReader GetXmlReader()
	    {
			//this XDocument will hold the total xml for the page, which is then returned to be transformed against the xslt
            XDocument xDocument = new XDocument();
			//the attribute here specifies the page title
	        xPageNode = new XElement("page", new XAttribute("title", "The Grisly Grotto - Deviant Minds Think Alike"));
	 
			//load the xml documents that contain user, blog and other site data
	        xUsersXml = XDocument.Load(string.Format("{0}\\blog\\xml\\users.xml", sPhysicalApplicationPath));
	        xBlogsXml = XDocument.Load(string.Format("{0}\\blog\\xml\\blogs.xml", sPhysicalApplicationPath));
	        xQuotesXml = XDocument.Load(string.Format("{0}\\blog\\xml\\quotes.xml", sPhysicalApplicationPath));      

	        //check for logged in status
            string sLoggingAuthor = CheckLogin();
            if(!string.IsNullOrEmpty(sLoggingAuthor) || !string.IsNullOrEmpty(GetProfileValue("loggedin")))
                xPageNode.Add(new XElement("loggedauthor", new XAttribute("name", string.IsNullOrEmpty(sLoggingAuthor) ? GetProfileValue("loggedin") : sLoggingAuthor)));
            //check for logout
            if (Request.Form["logout"] != null)
                SetProfileValue("loggedin", null);

            //Find the current page's author
            if (!string.IsNullOrEmpty(sLoggingAuthor))	//this function will log a user in if they have submitted correct details...
                sCurrentAuthor = sLoggingAuthor; //...and their username will be in session
	        else if (Request.QueryString["author"] != null && LinqUserExists(Request.QueryString["author"]))
	            sCurrentAuthor = Request.QueryString["author"]; //else the user may have submitted an author request via an author link
	        else
	            sCurrentAuthor = DefaultAuthor(); //finally just use the default author if all else fails (me)
	        AddAuthorDetails(); //adds the current authors username and logged status to the page

			//find if there is a single blog to be displayed, while also checking for and performing a blog post
	        string sTargetBlog = CheckBlogPost();
            if(sTargetBlog == null)
                sTargetBlog = CheckCommentPost();
            CheckLinksEdit();

	        //Sidebar content is added here
	        if (Request.Form["editor"] == null && (sTargetBlog != null || Request.Form["blogpost"] == null))
	        {
	            //only display the side bar if the editor is not shown
                xPageNode.Add(new XElement("userbox"));
	            AddContents();
                AddUserLinks(); 
	            AddUsersRecentBlogPosts();
	            AddUsersBlogHistory();
	        }

	        //Main content is added here
	        AddRandomImage();
	        AddQuote();
	        if (sTargetBlog != null)
	            AddSingleBlog(sTargetBlog); //display the single blog requested
	        else
	        {
	            if (Request.Form["editor"] != null || (sTargetBlog == null && Request.Form["blogpost"] != null))
	                AddEditor(Request.Form["hiddentitle"]); //show the editor or...
	            else if (Request.QueryString["blog"] != null)
	                AddSingleBlog(Request.QueryString["blog"]); //show a single blog or...
	            else if (Request.QueryString["month"] != null)
	                AddMonthsBlogs(Request.QueryString["month"]); //add all blogs for a month or...
	            else
	                AddRecentBlogs(); //add the recent blogs, generally the last 5
	        }

	        xDocument.Add(xPageNode);
	        return xDocument.CreateReader();
        }

        #region Profile (mention to be changeable, i.e. between session and a cookie based system)

        private string GetProfileValue(string sKey)
        {
            return Convert.ToString(Session[sKey]);
        }

        private void SetProfileValue(string sKey, string sValue)
        {
            Session[sKey] = sValue;
        }

        #endregion

        #region Preliminary checks

        /// <summary>
		/// Checks whether a user has attempted to login, by inspecting the form collection, and if so attempts to authenticate them
		/// </summary>
		/// <returns>true if their login was successful, and false otherwise</returns>
	    private string CheckLogin()
	    {
	        if (Request.Form["login"] != null)
	        {
	            List<string> lsValidatedName = LinqUserByCredentials(Request.Form["username"], Request.Form["password"]);
	            if (lsValidatedName.Count() != 0)
	            {
	                //success!
                    SetProfileValue("loggedin", lsValidatedName[0]);
                    return lsValidatedName[0];
	            }
	            else //failure!
	                xPageNode.Add(new XElement("error", new XAttribute("area", "login")));
	        }
	        return null;
	    }

		/// <summary>
		/// Adds the current authors details to the xml (name and logged in status)
		/// </summary>
	    private void AddAuthorDetails()
	    {
            if (GetProfileValue("loggedin") != null)
                xPageNode.Add(new XElement("author", new XAttribute("name", sCurrentAuthor), new XAttribute("logged", GetProfileValue("loggedin"))));
	        else
	            xPageNode.Add(new XElement("author", new XAttribute("name", sCurrentAuthor)));
	    }

		/// <summary>
		/// Returns the default author specified in the author xml
		/// </summary>
	    private string DefaultAuthor()
	    {
	        List<string> lsPrimaryAuthor = LinqPrimaryUser();
	        if (lsPrimaryAuthor.Count() > 0)
	            return lsPrimaryAuthor[0];
	        else
	            return string.Empty;
	    }

        /// <summary>
        /// Checks for an attempted blog post (by inspecting the form collection) and if so makes the attempt (validation and xml addition)
        /// </summary>
        /// <returns>true if there was an attempt and a new blog has been successfully posted/edited, false otherwise</returns>
	    private string CheckBlogPost()
	    {
            if (Request.Form["blogpost"] != null && GetProfileValue("loggedin") != null)
	        {
	            if(Request.Form["blogtitle"].Length == 0)
				{
					//a title for a blog is required
	                xPageNode.Add(new XElement("error", new XAttribute("area", "blogtitle")));
				}
	            else if (LinqSingleBlog(Request.Form["blogtitle"]).Count() > 0 && Request.Form["hiddentitle"] != Request.Form["blogtitle"])
				{
					//each blog must have a unique title, so check the entered title against existing blogs
	                xPageNode.Add(new XElement("error", new XAttribute("area", "uniquetitle")));
				}
	            if(Request.Form["blogtext"].Length == 0)
				{
					//a blog must have some text, obviously
	                xPageNode.Add(new XElement("error", new XAttribute("area", "blogtext")));
				}
	            if (Request.Form["blogtitle"].Length > 0 && Request.Form["blogtext"].Length > 0 && (LinqSingleBlog(Request.Form["blogtitle"]).Count() == 0 || Request.Form["hiddentitle"] == Request.Form["blogtitle"]))
	            {
					//all validation has succeeded, so compile the new blog xml node
	                XElement xNewBlog = new XElement("blog",
	                    new XAttribute("title", Request.Form["blogtitle"]),
                        new XAttribute("author", GetProfileValue("loggedin")),
	                    new XAttribute("date", DateTime.Now.ToUniversalTime()),
	                    new XText(Request.Form["blogtext"]));

	                if (Request.Form["hiddentitle"] != null)
	                {
						//if there is a hidden title field, then this is not a new blog but on old blog that is to be edited
						List<XElement> lxOldBlog = LinqSingleBlog(Request.Form["hiddentitle"]);
	                    if (lxOldBlog.Count == 0)
	                        xBlogsXml.Elements("blogs").Single().Add(xNewBlog); //add the blog as new
	                    else
	                    {
							//replace the older blog node in the xml, keeping the originals date
	                        xNewBlog.SetAttributeValue("date", lxOldBlog[0].Attribute("date").Value);
	                        LinqUpdateBlog(Request.Form["hiddentitle"], xNewBlog);
	                    }
	                }
	                else
	                    xBlogsXml.Elements("blogs").Single().Add(xNewBlog); //add the blog as new

					//save the blog xml to the file system, so the next page request will be updated (maybe some system caching here might be wise?)
                    xBlogsXml.Save(string.Format("{0}\\blog\\xml\\blogs.xml", sPhysicalApplicationPath));
	                return Request.Form["blogtitle"];
	            }
	        }
	        else if (Request.Form["blogpost"] != null)
			{
				//session has expired (maybe some coding to circumvent session dependance?)
	            xPageNode.Add(new XElement("error", new XAttribute("area", "session")));
			}
	        return null;
	    }

        /// <summary>
        /// Checks to see if there has been a comment post attempt (by inspecting the form collection), and if so, attempts to post the new 
        /// comment (checking validation and adding the comment to the xml)
        /// </summary>
        /// <returns>the string title of the blog the comment was posted against, whether or not the comment was successfully posted</returns>
        private string CheckCommentPost()
        {
            if (Request.Form["commentpost"] != null)
            {
                if (Request.Form["commentauthor"].Length == 0)
                {
                    //all comments must have an author specified
                    xPageNode.Add(new XElement("error", new XAttribute("area", "commentauthor")));
                }
                if (Request.Form["commenttext"].Length == 0)
                {
                    //all comments must have some text content
                    xPageNode.Add(new XElement("error", new XAttribute("area", "commenttext")));
                }
                if (Request.Form["commentauthor"].Length > 0 && Request.Form["commenttext"].Length > 0)
                {
                    //create the new comment as an xml node
                    XElement xNewComment = new XElement("comment",
                    new XAttribute("author", Request.Form["commentauthor"]),
                    new XAttribute("date", DateTime.Now.ToUniversalTime()),
                    new XText(Request.Form["commenttext"]));

					//add the new comment to the blog in question
					LinqAddCommentToBlog(Request.Form["commentblog"], xNewComment);
                    //save the blog xml to the file system, so the next page request will be updated (maybe some system caching here might be wise?)
                    xBlogsXml.Save(string.Format("{0}\\blog\\xml\\blogs.xml", sPhysicalApplicationPath));
		        }
        
			    return Request.Form["commentblog"];
            }

            return null;
        }

        /// <summary>
        /// Appends or deletes a new link category or link, if such a request can be found in the form collection and the user is logged in
        /// </summary>
        private void CheckLinksEdit()
        {
            if (GetProfileValue("loggedin") != null)
            {
                if (Request.Form["deletecategory"] != null)
                    LinqDeleteLinkCategory(sCurrentAuthor, Request.Form["categorytitle"]);
                else if (Request.Form["deletelink"] != null)
                    LinqDeleteLink(sCurrentAuthor, Request.Form["linktext"]);
                else if (Request.Form["addcategory"] != null)
                    LinqAddLinkCategory(sCurrentAuthor, Request.Form["categorytitle"]);
                else if (Request.Form["addlink"] != null)
                    LinqAddLink(sCurrentAuthor, Request.Form["hiddencategory"], Request.Form["linktext"], Request.Form["linkhref"]);
            }
            else if (Request.Form["deletecategory"] != null || Request.Form["deletelink"] != null || Request.Form["addcategory"] != null || Request.Form["addlink"] != null)
            {
                //if an attempt has been made to modify links, but there is no logged in author, then throw the session error message
                xPageNode.Add(new XElement("error", new XAttribute("area", "session")));
            }
        }

	    #endregion

	    #region Sidebar

        /// <summary>
        /// Adds a random image to the page xml, from the images in the site images/random folder
        /// </summary>
	    private void AddRandomImage()
	    {
	        string[] sPaths = System.IO.Directory.GetFiles(Request.PhysicalApplicationPath + "blog\\Images\\Random\\");
	        string sPath = sPaths[new Random().Next(0, sPaths.Length)].Substring(Request.PhysicalApplicationPath.Length).Replace("\\","/");
	        xPageNode.Add(new XElement("randomimage", new XAttribute("path", sPath)));
	    }

        /// <summary>
        /// adds the contents box to the page (contains links to each author)
        /// </summary>
	    private void AddContents()
	    {
	        List<string> lsAuthors = LinqAuthorList();

	        XElement xAuthorLinks = new XElement("authorlinks");
	        foreach (string sAuthor in lsAuthors)
	            xAuthorLinks.Add(new XElement("authorlink", new XAttribute("name", sAuthor)));

	        xPageNode.Add(xAuthorLinks);
	    }

        /// <summary>
        /// adds the links personal to each user to the page
        /// </summary>
        private void AddUserLinks()
        {
            List<XElement> lxCategories = LinqAuthorLinks(sCurrentAuthor);

            XElement xLinks = new XElement("links");
            foreach (XElement xCategory in lxCategories)
                xLinks.Add(xCategory);

            xPageNode.Add(xLinks);
        }

        /// <summary>
        /// Adds the recent blog posts by the current author to the page xml
        /// </summary>
	    private void AddUsersRecentBlogPosts()
	    {
	        XElement xRecentBlogs = new XElement("recentblogs");
	        
	        List<string> lsRecentBlogsList = LinqRecentBlogTitles(10, sCurrentAuthor);
	        foreach (string sRecentblog in lsRecentBlogsList)
	            xRecentBlogs.Add(new XElement("recentblog", new XAttribute("title", sRecentblog)));

	        xPageNode.Add(xRecentBlogs);
	    }

        /// <summary>
        /// Adds the monthly blog counts for the current author to the page xml
        /// </summary>
	    private void AddUsersBlogHistory()
	    {
	        XElement xHistory = new XElement("history");

	        List<string> lsMonthsList = LinqBlogsByMonthList(sCurrentAuthor);
	        foreach (string sMonth in lsMonthsList)
	        {
                int iCount = LinqBlogCountOfMonth(sMonth, sCurrentAuthor);

	            XElement xSpecificMonth = new XElement("month",
	                new XAttribute("text", string.Format("{0} ({1} Posts)", sMonth, iCount)), new XAttribute("link", DateTime.Parse(sMonth).ToString("MM/yyyy")));
	            xHistory.Add(xSpecificMonth);
	        }

	        xPageNode.Add(xHistory);
	    }

	    #endregion

	    #region Main content

        /// <summary>
        /// Adds a random quote to the page xml
        /// </summary>
	    private void AddQuote()
	    {
	        XElement xQuote = LinqRandomQuote();
	        xPageNode.Add(xQuote);
	    }

        /// <summary>
        /// Adds the editor xml to the page xml
        /// </summary>
        /// <param name="sBlog">The title of the blog to append the content of (if editing an existing blog</param>
	    private void AddEditor(string sBlog)
	    {
	        XElement xEditorNode = new XElement("editor");
	        if (sBlog != null)
	        {
	            List<XElement> lxOldBlog = LinqSingleBlog(sBlog);
	            if (lxOldBlog.Count == 0)
	            {
	                xPageNode.Add(new XElement("error", new XAttribute("area", "oldblog")));
	                return;
	            }
	            else
	                xEditorNode.Add(lxOldBlog[0]);
	        }
	        else if(Request.Form["blogpost"] != null)
	            xEditorNode.Add(new XElement("blog", new XAttribute("title", Request.Form["blogtitle"]), new XText(Request.Form["blogtext"])));
	        xPageNode.Add(xEditorNode);
	    }

        /// <summary>
        /// Adds the details of a single blog to a page
        /// </summary>
        /// <param name="sBlog">The title of the blog to be added</param>
	    private void AddSingleBlog(string sBlog)
	    {
	        List<XElement> lxSingleBlog = LinqSingleBlog(sBlog);
	        if (lxSingleBlog.Count() == 0)
	            xPageNode.Add(new XElement("error", new XAttribute("area", "blognotfound")));
	        else
	        {
	            AppendBlogList(lxSingleBlog, false);
                List<XElement> lxCommentsResults = LinqCommentsForBlog(sBlog);
	            
                XElement xComments = new XElement("comments");
                foreach (XElement comment in lxCommentsResults)
                {
                    DateTime dtPostedDate = DateTime.Parse(comment.Attribute("date").Value);
                    comment.SetAttributeValue("date", dtPostedDate.ToString("dddd, dd/MM/yyyy, h:mm tt"));

                    xComments.Add(comment);
                }
                xPageNode.Add(xComments);

                if (Request.Form["commentpost"] != null && (Request.Form["commentauthor"].Length == 0 || Request.Form["commenttext"].Length == 0))
                    xPageNode.Add(new XElement("commenteditor", new XAttribute("author", Request.Form["commentauthor"]), new XText(Request.Form["commenttext"])));
	        }
	    }

        /// <summary>
        /// Adds the details of all blogs of a given month to the page xml
        /// </summary>
        /// <param name="sMonth">The month to retrieve the blogs of the current author for</param>
	    private void AddMonthsBlogs(string sMonth)
	    {
            List<XElement> lxBlogResults = LinqBlogsFromMonth(sMonth, sCurrentAuthor);
	        AppendBlogList(lxBlogResults, true);
	    }

        /// <summary>
        /// Adds the recent blogs of the current author to the page (at present the last 5)
        /// </summary>
	    private void AddRecentBlogs()
	    {
            List<XElement> lxBlogResults = LinqRecentBlogs(5, sCurrentAuthor);
	        AppendBlogList(lxBlogResults, true);
	    }

        /// <summary>
        /// Adds a given list of blog details to the page. Used by the above blog addition functions
        /// </summary>
        /// <param name="lxBlogList">A list of blog details, as xelement xml nodes</param>
        /// <param name="bAddCommentCount">Whether the appended blogs should have their individual comment counts appended</param>
	    private void AppendBlogList(List<XElement> lxBlogList, bool bAddCommentCount)
	    {
	        XElement xBlogs = new XElement("blogs");
	        foreach (XElement xBlog in lxBlogList)
	        {
	            DateTime dtPostedDate = DateTime.Parse(xBlog.Attribute("date").Value);
	            if (dtPostedDate.ToString("dd MM yyyy") == DateTime.Now.ToString("dd MM yyyy"))
	                xBlog.SetAttributeValue("date", "Today, " + dtPostedDate.ToString("h:mm tt"));
	            else if (dtPostedDate.ToString("dd MM yyyy") == DateTime.Now.AddDays(-1).ToString("dd MM yyyy"))
	                xBlog.SetAttributeValue("date", "Yesterday, " + dtPostedDate.ToString("h:mm tt"));
	            else
	                xBlog.SetAttributeValue("date", dtPostedDate.ToString("dddd, dd/MM/yyyy, h:mm tt"));

                if (bAddCommentCount)
                {
                    int commentCount = LinqCommentCount(xBlog.Attribute("title").Value);
                    xBlog.Add(new XAttribute("comments", commentCount.ToString()));
                }

	            xBlogs.Add(xBlog);
	        }
	        xPageNode.Add(xBlogs);
	    }

	    #endregion

	    #region Linq

	    private List<string> LinqUserByCredentials(string sUsername, string sPassword)
	    {
            return xUsersXml.Element("users").Elements()
				.Where(user => user.Attribute("username").Value == sUsername
	            && user.Attribute("password").Value == sPassword).Select(author => author.Attribute("name").Value).ToList();
	    }

	    private bool LinqUserExists(string sName)
	    {
            return xUsersXml.Element("users").Elements()
				.Where(user => user.Attribute("name").Value == sName)
				.ToList().Count > 0;
	    }

	    private List<string> LinqPrimaryUser()
	    {
            return xUsersXml.Element("users").Elements()
				.Where(user => user.Attribute("primary") != null)
				.Select(author => author.Attribute("name").Value).ToList();
	    }

	    private void LinqUpdateBlog(string sOldtitle, XElement xNewBlog)
	    {
	        xBlogsXml.Elements("blogs").Elements()
				.Where(blog => blog.Attribute("title").Value == sOldtitle)
				.Single().ReplaceWith(xNewBlog);
	    }

        private void LinqDeleteLinkCategory(string sAuthor, string sCategoryTitle)
        {
            xUsersXml.Element("users").Elements()
                .Where(user => user.Attribute("name").Value == sAuthor).First()
                .Element("links")
                .Elements("category")
                .Where(element => element.Attribute("title").Value == sCategoryTitle).Remove();
            xUsersXml.Save(string.Format("{0}\\blog\\xml\\users.xml", sPhysicalApplicationPath));
        }

        private void LinqDeleteLink(string sAuthor, string sLinkText)
        {
            xUsersXml.Element("users").Elements()
                .Where(user => user.Attribute("name").Value == sAuthor).First()
                .Element("links")
                .Descendants("link")
                .Where(element => element.Attribute("text").Value == sLinkText).Remove();
            xUsersXml.Save(string.Format("{0}\\blog\\xml\\users.xml", sPhysicalApplicationPath));
        }

        private void LinqAddLinkCategory(string sAuthor, string sTitle)
        {
            xUsersXml.Element("users").Elements()
                .Where(user => user.Attribute("name").Value == sAuthor).First()
                .Element("links")
                .Add(new XElement("category",
                new XAttribute("title", sTitle)));
            xUsersXml.Save(string.Format("{0}\\blog\\xml\\users.xml", sPhysicalApplicationPath));
        }

        private void LinqAddLink(string sAuthor, string sCategory, string sText, string sHref)
        {
            xUsersXml.Element("users").Elements()
                .Where(user => user.Attribute("name").Value == sAuthor).First()
                .Element("links").Elements()
                .Where(element => element.Attribute("title").Value == sCategory).Single()
                .Add(new XElement("link", new XAttribute("text", sText), new XAttribute("href", sHref)));
            xUsersXml.Save(string.Format("{0}\\blog\\xml\\users.xml", sPhysicalApplicationPath));
        }

	    private List<string> LinqAuthorList()
	    {
	        return xBlogsXml.Element("blogs").Elements("blog")
				.OrderByDescending(author => author.Attribute("date").Value)
				.Select(author => author.Attribute("author").Value).Distinct().ToList();
	    }

        private List<XElement> LinqAuthorLinks(string sAuthor)
        {
            return xUsersXml.Element("users").Elements()
                .Where(user => user.Attribute("name").Value == sAuthor).First()
                .Element("links").Elements().ToList();
        }

	    private List<string> LinqRecentBlogTitles(int iCount, string sAuthor)
	    {
	        return xBlogsXml.Element("blogs").Elements("blog")
                .Where(blog => blog.Attribute("author").Value == sAuthor)
	            .OrderByDescending(blog => DateTime.Parse(blog.Attribute("date").Value))
                .Select(blog => blog.Attribute("title").Value).Take(iCount).ToList();
	    }

	    private List<string> LinqBlogsByMonthList(string sAuthor)
	    {
	        return xBlogsXml.Element("blogs").Elements("blog")
                .Where(blog => blog.Attribute("author").Value == sAuthor)
	            .OrderByDescending(blog => DateTime.Parse(blog.Attribute("date").Value))
	            .Select(blog => DateTime.Parse(blog.Attribute("date").Value).ToString("MMMM yyyy")).Distinct().ToList();
	    }

        private int LinqBlogCountOfMonth(string sMonth, string sAuthor)
	    {
	        return xBlogsXml.Element("blogs").Elements("blog")
				.Where(blog => DateTime.Parse(blog.Attribute("date").Value).ToString("MMMM yyyy") == sMonth &&
                     blog.Attribute("author").Value == sAuthor).ToList().Count();
	    }

	    private XElement LinqRandomQuote()
	    {
	        return xQuotesXml.Element("quotes").Elements()
				.ToList()[new Random().Next(0, xQuotesXml.Element("quotes").Elements().Count())];
	    }

	    private List<XElement> LinqSingleBlog(string sBlog)
	    {
	        return xBlogsXml.Element("blogs").Elements()
				.Where(singleblog => singleblog.Attribute("title").Value == sBlog)
				.ToList();
	    }

        private List<XElement> LinqCommentsForBlog(string sBlog)
        {
            return xBlogsXml.Element("blogs").Elements()
				.Where(singleblog => singleblog.Attribute("title").Value == sBlog)
				.Elements("comment").ToList();
        }

        private List<XElement> LinqBlogsFromMonth(string sMonth, string sAuthor)
	    {
	        return xBlogsXml.Element("blogs").Elements()
	            .Where(blog => DateTime.Parse(blog.Attribute("date").Value).ToString("MM/yyyy") == sMonth)
                .Where(blog => blog.Attribute("author").Value == sAuthor)
	            .OrderByDescending(blog => DateTime.Parse(blog.Attribute("date").Value))
	            .Select(blog => blog).ToList();
	    }

        private List<XElement> LinqRecentBlogs(int sCount, string sAuthor)
	    {
	        return xBlogsXml.Element("blogs").Elements()
				.Where(blog => blog.Attribute("author").Value == sAuthor)
	            .OrderByDescending(blog => DateTime.Parse(blog.Attribute("date").Value))
	            .Take(sCount).ToList();
	    }

        private int LinqCommentCount(string sBlog)
        {
            return xBlogsXml.Element("blogs").Elements()
				.Where(singleblog => singleblog.Attribute("title").Value == sBlog)
				.Elements("comment").ToList().Count();
        }

		private void LinqAddCommentToBlog(string sBlog, XElement xNewComment)
	    {
	        xBlogsXml.Element("blogs").Elements()
				.Where(singleblog => singleblog.Attribute("title").Value == sBlog)
				.Single().Add(xNewComment);
	    }
		
	    #endregion

	}
}
