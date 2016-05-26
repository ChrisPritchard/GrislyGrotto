using System.Linq;
using System.Web.Mvc;
using GrislyGrotto.Models;
using GrislyGrotto.Mvc;

namespace GrislyGrotto.Controllers
{

    public class UserController : XController
    {
        GrislyGrottoDBDataContext db;
        PredicateValidator InputContract;

        public UserController()
        {
            db = new GrislyGrottoDBDataContext();
            InputContract = new PredicateValidator();
        }

        public ActionResult Login(string Username, string Password)
        {
            InputContract.Validate(!string.IsNullOrEmpty(Username));
            InputContract.Validate(!string.IsNullOrEmpty(Password));
            if (!InputContract.Valid)
                return RedirectToAction("/");

            IQueryable<User> user = db.Users.Where(u => u.Username == Username && u.Password == Password);
            if (user.Count() > 0)
                Session["user"] = user.Single();

            return Redirect("/");
        }

        public ActionResult Logout()
        {
            Session["user"] = null;

            return Redirect("/");
        }

    }
}
