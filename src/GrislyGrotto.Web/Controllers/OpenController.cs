using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace GrislyGrotto
{
    public class OpenController : Controller
    {
        private const int _latestCount = 5;
        private readonly GrislyGrottoDbContext _db;

        public OpenController(GrislyGrottoDbContext dbContext)
        {
            _db = dbContext;
        }

        [HttpGet("")]
        [Route("page/{pageNum?}")]
        public async Task<IActionResult> Latest(int? pageNum)
        {
            var page = pageNum ?? 1;
			ViewBag.Page = page;

            var model = await _db.Posts.OrderByDescending(o => o.Date)
                .Include(o => o.Author).Include(o => o.Comments)
                .Skip((page - 1) * _latestCount)
                .Take(_latestCount).ToArrayAsync();

			return View(model);
        }
    }
}