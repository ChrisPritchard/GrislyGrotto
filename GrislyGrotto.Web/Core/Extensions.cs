using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;

namespace GrislyGrotto.Web.Core
{
    public static class Extensions
    {
        /// <summary>
        /// Converts an object to a given type, or the type default if conversion fails
        /// </summary>
        public static T As<T>(this object o)
        {
            try
            {
                return (T)Convert.ChangeType(o, typeof(T));
            }
            catch (Exception)
            {
                return default(T);
            }
        }

        public static IEnumerable<XElement> AsXElement(this Post post)
        {
            return post.AsXElement(false);
        }

        public static IEnumerable<XElement> AsXElement(this Post post, bool withComments)
        {
            yield return new XElement("Post",
               new XAttribute("ID", post.ID),
               new XAttribute("User", post.User.FullName),
               new XAttribute("Created", post.Created.ToWebFormat()),
               new XAttribute("Title", post.Title),
               new XAttribute("Status", post.Status),
               new XAttribute("CommentCount", post.Comments != null ? post.Comments.Count() : 0),
               new XText(post.Content));
            if (withComments && post.Comments != null)
                foreach (var comment in post.Comments)
                    yield return comment.AsXElement();
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

        private static XElement AsXElement(this Comment comment)
        {
            return new XElement("Comment",
               new XAttribute("Author", comment.Author),
               new XAttribute("Created", comment.Created.ToWebFormat()),
               new XText(comment.Content));
        }

        public static XElement AsXElement(this KeyValuePair<DateTime, int> monthPostCount)
        {
            return new XElement("MonthPostCount",
               new XAttribute("Year", monthPostCount.Key.Year),
               new XAttribute("Month", CultureInfo.CurrentCulture.DateTimeFormat.MonthNames[monthPostCount.Key.Month - 1]),
               new XAttribute("Count", monthPostCount.Value));
        }

        private static string ToWebFormat(this DateTime dateTime)
        {
            return dateTime.ToString("dddd, d MMMM yyyy, hh:mm tt");
        }

        private static string ToWebFormat(this TimeSpan timeSpan)
        {
            return (timeSpan.Days > 0 ? timeSpan.Days + " day" + (timeSpan.Days == 1 ? string.Empty : "s") + ", " : string.Empty)
                + (timeSpan.Hours > 0 ? timeSpan.Hours + " hour" + (timeSpan.Hours == 1 ? string.Empty : "s") + ", " : string.Empty)
                + timeSpan.Minutes + " minute" + (timeSpan.Minutes == 1 ? string.Empty : "s");
        }
    }
}