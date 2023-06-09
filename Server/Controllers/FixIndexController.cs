using Microsoft.AspNetCore.Mvc;
using Server.Db;

namespace Server.Controllers
{
    [Route("fix-index")]
    public class FixIndexController : Controller
    {
        private readonly TodoAppContext _context;

        public FixIndexController(TodoAppContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> FixIndex()
        {
            await _context.FixIndex();
            return NoContent();
        }
    }
}
