// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Utility.cs" company="Conflict Entertainment">
//   2013, Chris Pritchard
// </copyright>
// <summary>
//   Expresses shared methods that have no specific domain location
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace GrislyGrotto
{
    using System;
    using System.Text;
    using System.Web;

    using Data;

    /// <summary>
    /// Expresses shared methods that have no specific domain location
    /// </summary>
    public static class Utility
    {
        /// <summary>
        /// Creates a storable authentication cookie string from a database user
        /// </summary>
        /// <param name="user">The database user to source from</param>
        /// <returns>A base 64 string suitable for an authentication cookie</returns>
        public static string UserCookie(User user)
        {
            var token = user.Username + "|" + user.DisplayName;
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(token));
        }

        /// <summary>
        /// The current logged in user, if one exists, stored in the authentication cookie and retrieved via the context identity property
        /// </summary>
        /// <returns>A strongly typed user container</returns>
        public static LoggedUserDetails LoggedUser()
        {
            var context = HttpContext.Current;
            if (!context.Request.IsAuthenticated || context.User == null)
            {
                return null;
            }

            var value = context.User.Identity.Name;
            var text = Encoding.UTF8.GetString(Convert.FromBase64String(value));
            var segments = text.Split('|');

            return new LoggedUserDetails
            {
                Username = segments[0],
                DisplayName = segments[1]
            };
        }

        /// <summary>
        /// The current new zealand time (DateTime.Now converted to the NZ time zone)
        /// </summary>
        /// <returns>Returns a date time localized to New Zealand time</returns>
        public static DateTime CurrentNzTime()
        {
            var newZealandTime = TimeZoneInfo.FindSystemTimeZoneById("New Zealand Standard Time");
            return TimeZoneInfo.ConvertTime(DateTime.Now, newZealandTime);
        }

        /// <summary>
        /// A class to serve as a container for user details
        /// </summary>
        public class LoggedUserDetails
        {
            /// <summary>
            /// Gets or sets the users login name
            /// </summary>
            public string Username
            {
                get; set;
            }

            /// <summary>
            /// Gets or sets the users display name for rendering purposes
            /// </summary>
            public string DisplayName
            {
                get; set;
            }
        }
    }
}