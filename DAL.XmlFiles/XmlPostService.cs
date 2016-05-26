using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Xml;
using System.Xml.Linq;
using GrislyGrotto.Core;
using GrislyGrotto.Core.Entities;

namespace DAL.XmlFiles
{
    public class XmlPostService : IPostService
    {
        private const string postDirectory = "/Data/";
        private readonly IUserService userService;

        public XmlPostService(IUserService availableUserService)
        {
            userService = availableUserService;
        }

        private IEnumerable<Post> FindElements(string xpath, params string[] arguments)
        {
            var currentYear = DateTime.Now.Year;
            for (var i = currentYear; i >= 2006; i--)
            {
                var fileUrl = HttpContext.Current.Server.MapPath(postDirectory + i + ".xml");
                if (!File.Exists(fileUrl))
                    continue;
                var xml = new XmlDocument();
                xml.Load(fileUrl);
                var elements = xml.SelectNodes(string.Format(xpath, arguments));
                if (elements != null && elements.Count > 0)
                    foreach (XmlNode element in elements)
                        yield return PostFrom(element);
            }
            yield break;
        }

        private Post PostFrom(XmlNode postElement)
        {
            return new Post
                       {
                           ID = Int32.Parse(postElement.Attributes["ID"].Value),
                           Title = postElement.Attributes["Title"].Value,
                           User = userService.GetUserByFullName(postElement.Attributes["Author"].Value),
                           Created = DateTime.Parse(postElement.Attributes["Created"].Value),
                           Status = postElement.Attributes["Status"] != null ? (PostStatus)Enum.Parse(typeof(PostStatus), postElement.Attributes["Status"].Value) : PostStatus.Draft,
                           Content = postElement.InnerText,
                           Comments = CommentsIn(postElement)
                       };
        }

        private XmlNode ElementFrom(Post post)
        {
            var postElement = new XElement("Post",
                                           new XAttribute("ID", post.ID),
                                           new XAttribute("Author", post.User.FullName),
                                           new XAttribute("Title", post.Title),
                                           new XAttribute("Created", post.Created.ToString()),
                                           new XAttribute("Status", post.Status.ToString()),
                                           new XText(post.Content));
            foreach (var comment in post.Comments)
                postElement.Add(new XElement("Comment",
                    new XAttribute("Author", comment.Author),
                    new XAttribute("Created", comment.Created.ToString()),
                    new XText(comment.Content)));
            return new XmlDocument {InnerXml = postElement.ToString()}.DocumentElement;
        }

        private IEnumerable<Comment> CommentsIn(XmlNode postElement)
        {
            foreach (XmlNode commentElement in postElement.SelectNodes("Comment"))
                yield return new Comment
                                 {
                                     Author = commentElement.Attributes["Author"] != null ? commentElement.Attributes["Author"].Value : string.Empty,
                                     Created = commentElement.Attributes["Created"] != null ? DateTime.Parse(commentElement.Attributes["Created"].Value) : DateTime.MinValue,
                                     Content = commentElement.InnerText
                                 };
        }

        private XmlDocument XmlOf(int year)
        {
            var fileUrl = HttpContext.Current.Server.MapPath(postDirectory + year + ".xml");
            if(!File.Exists(fileUrl))
                return null;
            var xml = new XmlDocument();
            xml.Load(fileUrl);
            return xml;
        }

        private XmlDocument LatestXml
        {
            get
            {
                var document = new XmlDocument();

                var currentYear = DateTime.Now.Year;
                var fileUrl = HttpContext.Current.Server.MapPath(postDirectory + currentYear + ".xml");

                if (!File.Exists(fileUrl))
                {
                    document.InnerXml = "<?xml version=\"1.0\" encoding=\"utf-8\" ?><Posts />";
                    document.Save(fileUrl);
                }
                else
                    document.Load(fileUrl);

                return document;
            }
            set
            {
                var currentYear = DateTime.Now.Year;
                var fileUrl = HttpContext.Current.Server.MapPath(postDirectory + currentYear + ".xml");
                value.Save(fileUrl);
            }
        }

        private IEnumerable<Post> PostsIn(XmlDocument xml)
        {
            foreach (XmlNode post in xml.SelectNodes("//Post"))
                yield return PostFrom(post);
        }

        public IEnumerable<Post> GetLatest(User user, DateTime fromTime, int? count, PostStatus type)
        {
            var result = PostsIn(LatestXml).Where(post => 
                post.Created <= fromTime 
                && post.Status.Equals(type) 
                && (user == null || post.User.Equals(user)));
            return count.HasValue ? result.Take(count.Value) : result;
        }

        public Post GetSpecific(int postID)
        {
            var results = FindElements("//Post[@ID = {0}]", postID.ToString());
            return results.Count() > 0 ? results.First() : null;
        }

        public IEnumerable<Post> GetFromMonth(User user, int year, string monthName, PostStatus type)
        {
            return PostsIn(XmlOf(year)).Where(post => 
                post.Created.ToString("MMMM").Equals(monthName) 
                && post.Status.Equals(type) 
                && (user == null || post.User.Equals(user)));
        }

        public Dictionary<DateTime, int> GetMonthPostCounts(User user)
        {
            return new Dictionary<DateTime, int>();
        }

        public IEnumerable<Post> Search(User user, string searchTerm, PostStatus type)
        {
            yield break;
        }

        public Post AddPost(Post post)
        {
            return new Post();
        }

        public void UpdatePost(Post post)
        { }

        public void DeletePost(int postID)
        { }

        public void AddComment(Post post, Comment comment)
        { }
    }
}