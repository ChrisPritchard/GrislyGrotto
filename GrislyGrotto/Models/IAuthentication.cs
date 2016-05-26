using GrislyGrotto.Models.DTO;

namespace GrislyGrotto.Models
{
    public interface IAuthentication
    {
        bool TryLogin(string username, string password);
        bool UserIsLoggedIn();
        UserInfo GetLoggedInUser();
        void Logout();
    }
}
