using System.Web;
using GrislyGrotto.Infrastructure;
using GrislyGrotto.Infrastructure.Domain;

namespace GrislyGrotto.Website.Models.Defaults
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
            if (user == null || !user.Password.Equals(password))
                return false;

            HttpContext.Current.Session["LoggedUser"] = user;
            return true;
        }

        public bool UserIsLoggedIn()
        {
            return HttpContext.Current.Session["LoggedUser"] != null;
        }

        public User GetLoggedInUser()
        {
            var user = HttpContext.Current.Session["LoggedUser"];
            if (user == null)
                return null;
            else
                return (User)user;
        }

        public void Logout()
        {
            HttpContext.Current.Session["LoggedUser"] = null;
        }
    }
}
