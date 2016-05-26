using System.Web;
using GrislyGrotto.Models.Components;
using GrislyGrotto.Models.DTO;

namespace GrislyGrotto.Models.Defaults
{
    public class SessionAuthentication : IAuthentication
    {
        IUserRepository userRepository;

        public SessionAuthentication(IUserRepository userRepository)
        {
            this.userRepository = userRepository;
        }

        public bool TryLogin(string username, string password)
        {
            var user = userRepository.GetUserByUsername(username);
            if (user.IsNull() || !user.Password.Equals(password))
                return false;

            HttpContext.Current.Session["LoggedUser"] = user;
            return true;
        }

        public bool UserIsLoggedIn()
        {
            return !HttpContext.Current.Session["LoggedUser"].IsNull();
        }

        public UserInfo GetLoggedInUser()
        {
            var user = HttpContext.Current.Session["LoggedUser"];
            if (user.IsNull())
                return null;
            else
                return (UserInfo)user;
        }

        public void Logout()
        {
            HttpContext.Current.Session["LoggedUser"] = null;
        }
    }
}
