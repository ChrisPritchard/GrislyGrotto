using System;
using System.Collections.Generic;
using System.Linq;

namespace GrislyGrotto
{
    public static class Users
    {
        private static Dictionary<Tuple<string, string>, User> users = new Dictionary<Tuple<string, string>, User>
        {
            { Tuple.Create("aquinas", "***REMOVED***"), new User { DisplayName = "Christopher", ImageUrl = "http://grislygrotto.blob.core.windows.net/usercontentpub/facebook-small-chris.jpg" } },
            { Tuple.Create("pdc", "***REMOVED***"), new User { DisplayName = "Peter", ImageUrl = "http://grislygrotto.blob.core.windows.net/usercontentpub/facebook-small-pete.jpg" } }
        };

        public static string UserImageUrl(string displayName)
        {
            var user = users.Values.SingleOrDefault(o => o.DisplayName == displayName);
            return user != null ? user.ImageUrl : "#";
        }

        public static User FindUser(string username, string password)
        {
            var key = Tuple.Create(username, password);
            return users.ContainsKey(key) ? users[key] : null;
        }

        public class User
        {
            public string DisplayName { get; set; }
            public string ImageUrl { get; set; }
        }
    }
}