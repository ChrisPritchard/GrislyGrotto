using System;
using System.Web;

namespace GrislyGrotto.Framework.Data.Implementations
{
    public class CookieAuthenticationService : IAuthenticationService
    {
        public bool IsLoggedIn()
        {
            return HttpContext.Current.Request.Cookies["grislygrotto.co.nz"] != null;
        }

        public string LoggedInUser()
        {
            return IsLoggedIn() ? HttpContext.Current.Request.Cookies["grislygrotto.co.nz"].Value : null;
        }

        public void Login(string username, bool isPermanant)
        {
            HttpContext.Current.Response.Cookies.Add(new HttpCookie("grislygrotto.co.nz", username) { Expires = isPermanant ? DateTime.MaxValue : DateTime.Now.AddMinutes(20) });
        }

        public void Logout()
        {
            var context = HttpContext.Current;
            if (context.Request.Cookies["grislygrotto.co.nz"] != null)
                context.Response.Cookies["grislygrotto.co.nz"].Expires = DateTime.Now.AddDays(-1);
        }
    }
}