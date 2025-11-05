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
                return RedirectToAction(nameof(List));
            }

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> List()
        {
            // TODO: paginacja planów
            var plans = await _planService.GetPlansPaginatedAsync(pageSize: 20);

            ViewData["Plans"] = plans;
            ViewData["ActivePlanId"] = (await _planService.GetActivePlanAsync())?.Id ?? -1;

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Activate(int id)
        {
            // Znajdź plan po ID
            var plan = await _planService.GetPlanAsync(id);
            if (plan == null)
            {
                TempData["error"] = "Wystąpił błąd z wyborem planu. Spróbuj jeszcze raz.";
                return RedirectToAction(nameof(List));
            }

            // Ustaw jako aktywny
            await _planService.SetActivePlanAsync(plan);

            TempData["success"] = $"Plan '{plan.Id}' został aktywowany.";
            return RedirectToAction(nameof(List));
        }

        [HttpGet]
        public async Task<IActionResult> Deactivate(int id)
        {
            var activePlan = await _planService.GetActivePlanAsync();
            if (activePlan == null || activePlan.Id != id) 
            {
                TempData["error"] = "Nie można dezaktywować planu, który nie jest aktywny.";
                return RedirectToAction(nameof(List));
            }

            // Usuń aktywny plan
            await _planService.ClearActivePlanAsync();

            TempData["info"] = "Żaden plan nie jest obecnie aktywny.";
            return RedirectToAction(nameof(List));
        }

    }
}