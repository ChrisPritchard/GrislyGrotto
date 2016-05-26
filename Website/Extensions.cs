using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using GrislyGrotto.Core.Entities;

namespace GrislyGrotto.Website
{
    public static class Extensions
    {
        public static XElement AsXElement(this Post post)
        {
            return new XElement("Post",
               new XAttribute("ID", post.ID),
               new XAttribute("User", post.User.FullName),
               new XAttribute("Created", post.Created.ToWebFormat()),
               new XAttribute("Title", post.Title),
               new XAttribute("Status", post.Status),
               new XAttribute("CommentCount", post.Comments != null ? post.Comments.Count() : 0),
               new XText(post.Content));
        }

        public static XElement AsRecentTitleXElement(this Post post)
        {
            return new XElement("Post",
               new XAttribute("ID", post.ID),
               new XAttribute("User", post.User.FullName),
               new XAttribute("Created", DateTime.Now.Subtract(post.Created).ToWebFormat() + " ago"),
               new XAttribute("Title", string.IsNullOrEmpty(post.Title) ? "(No Title)" : post.Title),
               new XAttribute("Status", post.Status));
        }

        public static XElement AsXElement(this Comment comment)
        {
            return new XElement("Comment",
               new XAttribute("Author", comment.Author),
               new XAttribute("Created", comment.Created.ToWebFormat()),
               new XText(comment.Content));
        }

        public static XElement AsXElement(this User user)
        {
            return new XElement("User",
               new XAttribute("FullName", user.FullName),
               new XAttribute("LoginName", user.LoginName));
        }

        public static XElement AsXElement(this KeyValuePair<DateTime, int> monthPostCount)
        {
            return new XElement("MonthPostCount",
               new XAttribute("Year", monthPostCount.Key.Year),
               new XAttribute("Month", CultureInfo.CurrentCulture.DateTimeFormat.MonthNames[monthPostCount.Key.Month - 1]),
               new XAttribute("Count", monthPostCount.Value));
        }

        public static string ToWebFormat(this DateTime dateTime)
        {
            return dateTime.ToString("dddd, d MMMM yyyy, hh:mm tt");
        }

        public static string ToWebFormat(this TimeSpan timeSpan)
        {
            return (timeSpan.Days > 0 ? timeSpan.Days + " day" + (timeSpan.Days == 1 ? string.Empty : "s") + ", " : string.Empty)
                + (timeSpan.Hours > 0 ? timeSpan.Hours + " hour" + (timeSpan.Hours == 1 ? string.Empty : "s") + ", " : string.Empty)
                + timeSpan.Minutes + " minute" + (timeSpan.Minutes == 1 ? string.Empty : "s");
        }
    }
}