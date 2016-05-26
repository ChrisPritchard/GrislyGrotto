using System;
using System.Collections.Generic;
using System.Xml.Linq;
using GrislyGrotto.Infrastructure.Domain;
using GrislyGrotto.Website.Models.ViewModels;

namespace GrislyGrotto.Website.Models
{
    public class XElementMapper
    {
        public XElement Posts(IEnumerable<PostWithCommentCount> posts)
        {
            var postsXml = new XElement("Posts");
            foreach (var post in posts)
            {
                postsXml.Add(PostAsXElement(post, true));
            }
            return postsXml;
        }

        public XElement Comments(IEnumerable<Comment> comments)
        {
            var commentsXml = new XElement("Comments");
            foreach (Comment comment in comments)
            {
                commentsXml.Add(CommentAsXElement(comment));
            }
            return commentsXml;
        }

        public XElement Authors(IEnumerable<User> allUsers, string loggedUsername, string userFullname)
        {
            var authorsXml = new XElement("AuthorDetails");

            foreach (User user in allUsers)
            {
                var author = new XElement("Author",
                        new XAttribute("Username", user.Username),
                        new XAttribute("Fullname", user.Fullname));

                if (!string.IsNullOrEmpty(loggedUsername) && user.Username.Equals(loggedUsername))
                    author.Add(new XAttribute("LoggedIn", "true"));

                if (!string.IsNullOrEmpty(userFullname) && user.Fullname.Equals(userFullname))
                    author.Add(new XAttribute("Current", "true"));

                authorsXml.Add(author);
            }

            return authorsXml;
        }

        public XElement PostAsXElement(Post post)
        {
            return PostAsXElement(post, true);
        }

        public XElement PostAsXElement(PostWithCommentCount post, bool formatDate)
        {
            return new XElement("Post",
                new XAttribute("PostID", post.PostID),
                new XAttribute("Author", post.Author),
                new XAttribute("Title", post.Title),
                new XAttribute("EntryDate", formatDate ? post.EntryDate.AsFormattedDate() : post.EntryDate.ToString()),
                new XAttribute("CommentCount", post.CommentCount.ToString()),
                new XText(post.Content.Trim()));
        }

        public XElement PostAsXElement(Post post, bool formatDate)
        {
            return new XElement("Post",
                new XAttribute("PostID", post.PostID),
                new XAttribute("Author", post.Author),
                new XAttribute("Title", post.Title),
                new XAttribute("EntryDate", formatDate ? post.EntryDate.AsFormattedDate() : post.EntryDate.ToString()),
                new XText(post.Content.Trim()));
        }

        public XElement CommentAsXElement(Comment comment)
        {
            return CommentAsXElement(comment, true);
        }

        public XElement CommentAsXElement(Comment comment, bool formatDate)
        {
            return new XElement("Comment",
                new XAttribute("PostID", comment.PostID),
                new XAttribute("Author", comment.Author),
                new XAttribute("EntryDate", formatDate ? comment.EntryDate.AsFormattedDate() : comment.EntryDate.ToString()),
                new XText(comment.Text.Trim()));
        }

        public XElement QuoteAsXElement(Quote quote)
        {
            return new XElement("Quote",
                new XAttribute("Author", quote.Author),
                new XText(quote.Text.Trim()));
        }

        internal XElement RecentEntries(IEnumerable<RecentEntry> recentEntries)
        {
            var recentEntriesXml = new XElement("RecentEntries");
            foreach (var recentEntry in recentEntries)
            {
                TimeSpan timeSinceBlogged = DateTime.Now.Subtract(recentEntry.EntryDate);
                recentEntriesXml.Add(new XElement("Entry",
                    new XAttribute("PostID", recentEntry.PostID),
                    new XAttribute("Title", recentEntry.Title),
                    new XAttribute("HowRecent", timeSinceBlogged.AsFormattedText())));
            }
            return recentEntriesXml;
        }

        internal XElement MonthPostCounts(IEnumerable<MonthPostCount> monthPostCounts)
        {
            XElement monthPostCountsXml = new XElement("MonthBlogCounts");
            foreach (var monthPostCount in monthPostCounts)
            {
                monthPostCountsXml.Add(new XElement("Month",
                    new XAttribute("Year", monthPostCount.Year),
                    new XAttribute("Month", monthPostCount.Month),
                    new XAttribute("Count", monthPostCount.PostCount)));
            }
            return monthPostCountsXml;
        }
    }
}
