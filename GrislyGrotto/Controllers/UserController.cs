using System.Web.Mvc;
using GrislyGrotto.Models;
using GrislyGrotto.Models.Components;
using GrislyGrotto.Mvc;

namespace GrislyGrotto.Controllers
{
    public class UserController : XController
    {
        IAuthentication authentication;
        PredicateValidator validator;

        public UserController(IAuthentication authentication)
        {
            this.authentication = authentication;

            validator = new PredicateValidator();
        }

        /// <summary>
        /// attempt a login. Redirect to the homepage on success or failure
        /// </summary>
        public ActionResult Login(string Username, string Password)
        {
            validator.Validate(!string.IsNullOrEmpty(Username));
            validator.Validate(!string.IsNullOrEmpty(Password));
            if (!validator.Valid)
                return RedirectToAction("/");

            authentication.TryLogin(Username, Password);

            return Redirect("/");
        }

        /// <summary>
        /// Logout the current user, redirect to homepage
        /// </summary>
        public ActionResult Logout()
        {
            authentication.Logout();

            return Redirect("/");
        }
    }
}
