using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using SOK.Application.Services.Interface;
using SOK.Domain.Entities.Parish;
using SOK.Web.Filters;

namespace SOK.Web.Controllers
{
    [Authorize]
    [ActivePage("Calendar")]
    public class CalendarController : Controller
    {
        private readonly IPlanService _planService;

        public CalendarController(IPlanService planService)
        {
            _planService = planService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            Plan? plan = await _planService.GetActivePlanAsync();

            if (plan == null)
            {
                return RedirectToAction(nameof(PlansController.Index), "Plans");
            }

            return View();
        }
    }
}