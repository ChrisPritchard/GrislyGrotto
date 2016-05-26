using GrislyGrotto.Data;
using GrislyGrotto.ViewModels.Account;
using System.Linq;
using System.Web.Mvc;
using System.Web.Security;

namespace GrislyGrotto.Controllers
{
    public class AccountController : Controller
    {
        readonly GrislyGrottoContext database;

        public AccountController()
        {
            database = new GrislyGrottoContext();
        }

        public ActionResult Login()
        {
            return View(new LoginViewModel());
        }

        [HttpPost]
        public ActionResult Login(LoginViewModel viewModel)
        {
            if (string.IsNullOrWhiteSpace(viewModel.Username))
                ModelState.AddModelError("Username", "Username cannot be blank");
            if (string.IsNullOrWhiteSpace(viewModel.Password))
                ModelState.AddModelError("Password", "Password cannot be blank");

            if (!ModelState.IsValid)
                return View(viewModel);

            var user = database.Users.SingleOrDefault(u => u.Username.Equals(viewModel.Username));
            if (user != null && user.IsPasswordAMatch(viewModel.Password))
            {
                FormsAuthentication.SetAuthCookie(Utility.UserCookie(user), true);
                return RedirectToAction("Latest", "Home");
            }

            ModelState.AddModelError("Username", "Username and/or Password are invalid");
            return View(viewModel);
        }

        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return View();
        }
    }
}
