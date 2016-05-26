namespace GrislyGrotto.Framework.Data
{
    public interface IAuthenticationService
    {
        bool IsLoggedIn();
        string LoggedInUser();
        void Login(string username, bool isPermanant);
        void Logout();
    }
}