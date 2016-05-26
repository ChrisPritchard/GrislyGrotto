using System.Web;
using System.Xml;
using GrislyGrotto.Core;
using GrislyGrotto.Core.Entities;

namespace DAL.XmlFiles
{
    public class XmlUserService : IUserService
    {
        private const string relativeFilePath = "/Data/AuthorDetails.xml";
        private const string characters = "abcdefghijklmnopqrstuvwsyza01234567890";
        private readonly XmlDocument users;

        public XmlUserService()
        {
            users = new XmlDocument();
            users.Load(HttpContext.Current.Server.MapPath(relativeFilePath));
        }

        public User GetUserByFullName(string fullName)
        {
            var user = users.SelectSingleNode(string.Format("/Authors/Author[@Name='{0}']", fullName));
            if(user == null)
                return null;
            return new User
                       {
                           FullName = fullName,
                           LoginName = user.Attributes["Username"].Value,
                           LoginPassword = user.Attributes["Password"].Value
                       };
        }

        public User Validate(string loginName, string loginPassword)
        {
            var adjustedPassword = string.Empty;
            for (var i = 0; i < loginPassword.Length; i++)
                adjustedPassword += characters[characters.IndexOf(loginPassword[i]) + 1];

            var user = users.SelectSingleNode(string.Format("/Authors/Author[@Username='{0}' and @Password='{1}']", loginName, adjustedPassword));
            if (user == null)
                return null;
            return new User
            {
                FullName = user.Attributes["Name"].Value,
                LoginName = loginName,
                LoginPassword = adjustedPassword
            };
        }

        public User GetUserByLoginName(string loginName)
        {
            var user = users.SelectSingleNode(string.Format("/Authors/Author[@Username='{0}']", loginName));
            if (user == null)
                return null;
            return new User
            {
                FullName = user.Attributes["Name"].Value,
                LoginName = loginName,
                LoginPassword = user.Attributes["Password"].Value
            };
        }
    }
}