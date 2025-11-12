using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SOK.Web.Filters;

namespace SOK.Web.Controllers
{
    [Authorize]
    [ActivePage("Plans")]
    public class SettingsController : Controller
    {
        public SettingsController()
        {
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }
    }
}