using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SOK.Infrastructure.Persistence.Context;
using SOK.Web.Filters;
using System.Diagnostics;

namespace SOK.Web.Controllers
{
    [Authorize]
    [ActivePage("Home")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ParishDbContext _context;

        public HomeController(ILogger<HomeController> logger, ParishDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            var submission = _context.Submissions.FirstOrDefault();
            ViewData["test"] = submission?.Visit?.Id;
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}
