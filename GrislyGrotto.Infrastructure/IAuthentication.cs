using GrislyGrotto.Infrastructure.Domain;

namespace GrislyGrotto.Infrastructure
{
    public interface IAuthentication
    {
        bool TryLogin(string username, string password);
        bool UserIsLoggedIn();
        User GetLoggedInUser();
        void Logout();
    }
}
