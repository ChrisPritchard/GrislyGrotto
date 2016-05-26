using System.Web.Mvc;
using GrislyGrotto.Infrastructure;
using GrislyGrotto.Website.Models;
using Mvc.XslViewEngine;

namespace GrislyGrotto.Website.Controllers
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
