using GrislyGrotto.ViewModels;
using System.Security.Claims;
using System.Web;
using System.Web.Mvc;

namespace GrislyGrotto.Controllers
{
    public class AccountController : Controller
    {
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = Users.FindUser(model.Username, model.Password);
            if (user == null)
            {
                ModelState.AddModelError("Username", "Username and Password not recognised");
                return View(model);
            }

            var identity = new ClaimsIdentity(new[] {
                new Claim(ClaimTypes.Name, user.DisplayName)
            }, "Cookie");
            Request.GetOwinContext().Authentication.SignIn(identity);

            var returnUrl = Request.QueryString["ReturnUrl"] ?? "/";
            return Redirect(returnUrl);
        }
    }
}