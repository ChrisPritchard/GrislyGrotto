namespace GrislyGrotto.Framework
{
    public interface INavigationService
    {
        void RedirectToHome(bool forFailedLoginAttempt = false);
        void RedirectToPost(int postID);
    }
}