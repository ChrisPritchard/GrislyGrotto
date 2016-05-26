using System.Web;

namespace GrislyGrotto.Framework
{
    public class ContextNavigationService : INavigationService
    {
        public void RedirectToHome(bool forFailedLoginAttempt = false)
        {
            HttpContext.Current.Response.Redirect("/" + (forFailedLoginAttempt ? "LoginFailed" : string.Empty));
        }

        public void RedirectToPost(int postID)
        {
            HttpContext.Current.Response.Redirect("/posts/" + postID);
        }
    }
}