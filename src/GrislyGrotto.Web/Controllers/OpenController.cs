using Microsoft.AspNetCore.Mvc;

namespace GrislyGrotto
{
    public class OpenController : Controller
    {
        [HttpGet("")]
        public IActionResult Latest()
        {
            return View();
        }
    }
}