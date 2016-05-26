using System;
using System.Collections.Generic;
using System.Xml.Linq;
using GrislyGrotto.Models.DTO;

namespace GrislyGrotto.Models.Components
{
    public class DtoToXElementMapper
    {
        public XElement Posts(PostInfo[] postsArray)
        {
            var posts = new XElement("Posts");
            foreach (PostInfo post in postsArray)
            {
                posts.Add(PostAsXElement(post));
            }
            return posts;
        }

        public XElement Comments(CommentInfo[] commentsArray)
        {
            var comments = new XElement("Comments");
            foreach (CommentInfo comment in commentsArray)
            {
                comments.Add(CommentAsXElement(comment));
            }
            return comments;
        }

        public XElement Months(Dictionary<MonthInfo, int> postHistory)
        {
            XElement monthPostCounts = new XElement("MonthBlogCounts");
            foreach (var postCountForMonth in postHistory)
            {
                monthPostCounts.Add(new XElement("Month",
                    new XAttribute("Year", postCountForMonth.Key.Year),
                    new XAttribute("Month", postCountForMonth.Key.Month.ToString()),
                    new XAttribute("Count", postCountForMonth.Value)));
            }
            return monthPostCounts;
        }

        public XElement Entries(PostInfo[] postHistory)
        {
            var recentEntries = new XElement("RecentEntries");
            foreach (var entry in postHistory)
            {
                TimeSpan timeSinceBlogged = DateTime.Now.Subtract(entry.EntryDate);
                recentEntries.Add(new XElement("Entry",
                    new XAttribute("PostID", entry.PostID), 
                    new XAttribute("Title", entry.Title), 
                    new XAttribute("HowRecent", timeSinceBlogged.AsFormattedText())));
            }
            return recentEntries;
        }

        public XElement Authors(UserInfo[] allUsers, string loggedUsername, string userFullname)
        {
            var authors = new XElement("AuthorDetails");

            foreach (UserInfo user in allUsers)
            {
                var author = new XElement("Author",
                        new XAttribute("Username", user.Username),
                        new XAttribute("Fullname", user.Fullname));

                if (!string.IsNullOrEmpty(loggedUsername) && user.Username.Equals(loggedUsername))
                    author.Add(new XAttribute("LoggedIn", "true"));

                if (!string.IsNullOrEmpty(userFullname) && user.Fullname.Equals(userFullname))
                    author.Add(new XAttribute("Current", "true"));

                authors.Add(author);
            }

            return authors;
        }

        public XElement PostAsXElement(PostInfo post)
        {
            return new XElement("Post",
                new XAttribute("PostID", post.PostID),
                new XAttribute("Author", post.Author.Fullname),
                new XAttribute("Title", post.Title),
                new XAttribute("EntryDate", post.EntryDate.AsFormattedDate()),
                new XAttribute("CommentCount", post.CommentCount.ToString()),
                new XText(post.Content.Trim()));
        }

        public XElement CommentAsXElement(CommentInfo comment)
        {
            return new XElement("Comment",
                new XAttribute("CommentID", comment.CommentID),
                new XAttribute("Author", comment.Author),
                new XAttribute("EntryDate", comment.EntryDate.AsFormattedDate()),
                new XText(comment.Text.Trim()));
        }

        public XElement QuoteAsXElement(QuoteInfo quote)
        {
            return new XElement("Quote",
                new XAttribute("Author", quote.Author),
                new XText(quote.Text.Trim()));
        }
    }
}
