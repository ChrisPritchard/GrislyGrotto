using System;
using System.Collections;
using System.Collections.Generic;
using System.Web.Configuration;
using System.Linq;
using System.Xml.Linq;
using System.Web;

namespace GrislyGrotto.Models
{
    public class Util
    {
        public static T Config<T>(string sKey)
        {
            return To<T>(WebConfigurationManager.AppSettings[sKey]);
        }

        public static T To<T>(object o)
        {
            return (T)Convert.ChangeType(o, typeof(T));
        }

        public static User LoggedInAuthor()
        {
            if (HttpContext.Current.Session["user"] != null)
                return (User)HttpContext.Current.Session["user"];
            else
                return null;
        }

        /// <summary>
        /// Returns author details, plus logged in X
        /// </summary>
        public static XElement AuthorDetails(string sCurrentAuthor, GrislyGrottoDBDataContext db)
        {
            XElement xAuthors = new XElement("AuthorDetails");

            User oLoggedAuthor = Util.LoggedInAuthor();
            foreach (User user in db.Users)
            {
                XElement xAuthor = new XElement("Author",
                        new XAttribute("AuthorID", user.UserID),
                        new XAttribute("Fullname", user.Fullname));

                if (!oLoggedAuthor.IsNull() && user.UserID == oLoggedAuthor.UserID)
                    xAuthor.Add(new XAttribute("LoggedIn", "true"));
                if (!string.IsNullOrEmpty(sCurrentAuthor) && user.Fullname == sCurrentAuthor)
                    xAuthor.Add(new XAttribute("Current", "true"));

                xAuthors.Add(xAuthor);
            }

            return xAuthors;
        }

        /// <summary>
        /// Returns blog history for X Author or All Authors
        /// </summary>
        public static XElement BlogHistory(string Author, GrislyGrottoDBDataContext db)
        {
            XElement xHistory = new XElement("History");

            var recentEntries = db.Blogs.Where(b => string.IsNullOrEmpty(Author) || b.User.Fullname == Author).
                OrderByDescending(b => b.EntryDate).
                Select(b => new { b.BlogID, b.Title, b.EntryDate }).
                Take(10).ToList();

            XElement xRecentEntries = new XElement("RecentEntries");
            foreach (var entry in recentEntries)
            {
                TimeSpan tsHowRecent = DateTime.Now.Subtract(entry.EntryDate);
                string sHowRecent = (tsHowRecent.Days > 0 ? tsHowRecent.Days + " day" + (tsHowRecent.Days == 1 ? string.Empty : "s") + ", " : string.Empty)
                    + (tsHowRecent.Hours > 0 ? tsHowRecent.Hours + " hour" + (tsHowRecent.Hours == 1 ? string.Empty : "s") + ", " : string.Empty)
                    + tsHowRecent.Minutes + " minute" + (tsHowRecent.Minutes == 1 ? string.Empty : "s") + " ago";
                xRecentEntries.Add(new XElement("Entry", new XAttribute("BlogID", entry.BlogID), new XAttribute("Title", entry.Title), new XAttribute("HowRecent", sHowRecent)));
            }
            xHistory.Add(xRecentEntries);

            var blogsByMonth = db.Blogs.Where(b => string.IsNullOrEmpty(Author) || b.User.Fullname == Author).
                Select(b => new { b.EntryDate.Month, b.EntryDate.Year }).
                Distinct().
                OrderByDescending(b => new DateTime(b.Year, b.Month, 1)).ToList();

            XElement xMonthBlogCounts = new XElement("MonthBlogCounts");
            foreach (var month in blogsByMonth)
            {
                int iBlogCount = db.Blogs.Where(b => string.IsNullOrEmpty(Author) || b.User.Fullname == Author).
                    Where(b => b.EntryDate.Month == month.Month && b.EntryDate.Year == month.Year).
                    Count();
                xMonthBlogCounts.Add(new XElement("Month",
                    new XAttribute("Year", month.Year),
                    new XAttribute("Month", ((Months)month.Month).ToString()),
                    new XAttribute("Count", iBlogCount)));
            }
            xHistory.Add(xMonthBlogCounts);

            return xHistory;
        }

        /// <summary>
        /// Returns a quote at a random index
        /// </summary>
        public static XElement RandomQuote(GrislyGrottoDBDataContext db)
        {
            int iCount = db.Quotes.Count();
            Random o = new Random();
            int iSelected = o.Next(0, iCount);
            return db.Quotes.Take(iSelected + 1).ToList().Last().AsXElement();
        }

        /// <summary>
        /// Gets top X blogs by Y author, or by any author
        /// </summary>
        public static XElement LatestBlogs(string Author, GrislyGrottoDBDataContext db)
        {
            List<Blog> blogsList;
            blogsList = db.Blogs.Where(b => string.IsNullOrEmpty(Author) || b.User.Fullname == Author).
                OrderByDescending(b => b.EntryDate).Take(5).ToList();

            XElement xBlogsElement = new XElement("Blogs");
            foreach (Blog blog in blogsList)
            {
                XElement xBlog = blog.AsXElement();
                xBlog.Add(new XAttribute("Comments", db.Comments.Where(c => c.Blog == blog).Count()));
                xBlogsElement.Add(xBlog);
            }

            return xBlogsElement;
        }

    }
}
