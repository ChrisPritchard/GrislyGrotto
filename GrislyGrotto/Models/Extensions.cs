using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Xml.Linq;

namespace GrislyGrotto.Models
{
    public static class Extensions
    {
        public static XElement AsXElement(this Blog blog)
        {
            return blog.AsXElement(true);
        }

        public static XElement AsXElement(this Blog blog, bool bFormatDate)
        {
            return new XElement("Blog",
                new XAttribute("BlogID", blog.BlogID),
                new XAttribute("Title", blog.Title),
                new XAttribute("Tags", blog.Tags),
                new XAttribute("Author", blog.User.Fullname),
                new XAttribute("EntryDate", bFormatDate ? blog.EntryDate.AsFormattedDate() : blog.EntryDate.ToString()),
                new XText(blog.Content.Trim()));
        }

        public static XElement AsXElement(this Comment comment)
        {
            return comment.AsXElement(true);
        }

        public static XElement AsXElement(this Comment comment, bool bFormatDate)
        {
            return new XElement("Comment",
                new XAttribute("CommentID", comment.CommentID),
                new XAttribute("Author", comment.Author),
                new XAttribute("EntryDate", bFormatDate ? comment.EntryDate.AsFormattedDate() : comment.EntryDate.ToString()),
                new XText(comment.Content.Trim()));
        }

        public static XElement AsXElement(this Quote quote)
        {
            return new XElement("Quote",
                new XAttribute("Author", quote.Author),
                new XText(quote.Content.Trim()));
        }

        public static string AsFormattedDate(this DateTime dtDate)
        {
            if (dtDate.ToString("dd MM yyyy") == DateTime.Now.ToString("dd MM yyyy"))
                return "Today, " + dtDate.ToString("h:mm tt");
            else if (dtDate.ToString("dd MM yyyy") == DateTime.Now.AddDays(-1).ToString("dd MM yyyy"))
                return "Yesterday, " + dtDate.ToString("h:mm tt");
            else
                return dtDate.ToString("dddd, dd/MM/yyyy, h:mm tt");
        }

        public static bool IsNull(this User o)
        {
            return o == null;
        }
    }
}
