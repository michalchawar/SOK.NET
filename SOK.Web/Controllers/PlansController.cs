using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using SOK.Application.Common.DTO;
using SOK.Application.Services.Interface;
using SOK.Domain.Entities.Parish;
using SOK.Domain.Enums;
using SOK.Web.Filters;
using SOK.Web.ViewModels.Parish;

namespace SOK.Web.Controllers
{
    [AuthorizeRoles(Role.Administrator, Role.Priest)]
    [ActivePage("Plans")]
    public class PlansController : Controller
    {
        private readonly IPlanService _planService;
        private readonly IParishMemberService _parishMemberService;

        public PlansController(IPlanService planService, IParishMemberService parishMemberService)
        {
            _planService = planService;
            _parishMemberService = parishMemberService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var plans = await _planService.GetPlansPaginatedAsync(pageSize: 20);

            ViewData["Plans"] = plans;
            ViewData["ActivePlanId"] = (await _planService.GetActivePlanAsync())?.Id ?? -1;

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Activate(int id)
        {
            // Znajdź plan po ID
            var plan = await _planService.GetPlanAsync(id);
            if (plan == null)
            {
                TempData["error"] = "Wystąpił błąd z wyborem planu. Spróbuj jeszcze raz.";
                return RedirectToAction(nameof(Index));
            }

            // Ustaw jako aktywny
            await _planService.SetActivePlanAsync(plan);

            TempData["success"] = $"Plan '{plan.Name}' został aktywowany.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Deactivate(int id)
        {
            var activePlan = await _planService.GetActivePlanAsync();
            if (activePlan == null || activePlan.Id != id)
            {
                TempData["error"] = "Nie można dezaktywować planu, który nie jest aktywny.";
                return RedirectToAction(nameof(Index));
            }

            // Usuń aktywny plan
            await _planService.ClearActivePlanAsync();

            TempData["info"] = "Żaden plan nie jest obecnie aktywny.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var vm = new PlanVM
            {
                Priests = await GetPriests()
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PlanVM model)
        {
            if (!ModelState.IsValid)
            {
                TempData["error"] = "Popraw błędy w formularzu.";
                return View(model);
            }

            try
            {
                ScheduleVM? defaultSchedule = model.Schedules.SingleOrDefault(s => s.IsDefault);
            }
            catch (InvalidOperationException)
            {
                ModelState.AddModelError("Schedule", "Plan może mieć maksymalnie jeden domyślny harmonogram.");
                return View(model);
            }

            var schedules = model.Schedules.Select(s => new PlanScheduleDto() { Id = s.Id, Name = s.Name, ShortName = s.ShortName, IsDefault = s.IsDefault, Color = s.Color });
            var priests = model.Priests.Where(p => p.IsActive).Select(p => new PlanPriestDto() { Id = p.Id, DisplayName = p.DisplayName });

            PlanActionRequestDto request = new()
            {
                Id = null,
                Name = model.Name,
                Schedules = schedules,
                ActivePriests = priests,
                Author = await _parishMemberService.GetParishMemberAsync(this.User),
            };

            try
            {
                await _planService.CreatePlanAsync(request);
            }
            catch (Exception)
            {
                TempData["error"] = "Wystąpił problem przy tworzeniu planu. Skontaktuj się z administratorem.";
                return View(model);
            }
            TempData["success"] = "Plan został utworzony pomyślnie.";
            return RedirectToAction(nameof(Index));
        }
        
        [HttpGet("edit/{id}")]
        public async Task<IActionResult> Edit(int id)
        {
            Plan? plan = await _planService.GetPlanAsync(id);
            if (plan is null)
                return NotFound();

            var vm = new PlanVM
            {
                Name = plan.Name,
                Priests = await GetPriests(plan.Id),
                Schedules = [..plan.Schedules.Select(s => new ScheduleVM()
                {
                    Id = s.Id,
                    Name = s.Name,
                    ShortName = s.ShortName,
                    IsDefault = s.Id == plan.DefaultScheduleId,
                    Color = s.Color
                })]
            };

            return View(vm);
        }

        [HttpPost("edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, PlanVM model)
        {
            if (!ModelState.IsValid)
            {
                TempData["error"] = "Popraw błędy w formularzu.";
                return View(model);
            }

            Plan? plan = await _planService.GetPlanAsync(id);
            if (plan is null)
                return NotFound();

            try
            {
                ScheduleVM? defaultSchedule = model.Schedules.SingleOrDefault(s => s.IsDefault);
            }
            catch (InvalidOperationException)
            {
                ModelState.AddModelError("Schedule", "Plan może mieć maksymalnie jeden domyślny harmonogram.");
                return View(model);
            }

            var schedules = model.Schedules.Select(s => new PlanScheduleDto() { Id = s.Id, Name = s.Name, ShortName = s.ShortName, IsDefault = s.IsDefault, Color = s.Color });
            var priests = model.Priests.Where(p => p.IsActive).Select(p => new PlanPriestDto() { Id = p.Id, DisplayName = p.DisplayName });

            PlanActionRequestDto request = new()
            {
                Id = plan.Id,
                Name = model.Name,
                Schedules = schedules,
                ActivePriests = priests,
                Author = await _parishMemberService.GetParishMemberAsync(this.User),
            };

            try
            {
                await _planService.UpdatePlanAsync(request);
            }
            catch (Exception)
            {
                TempData["error"] = "Wystąpił problem przy aktualizacji planu. Skontaktuj się z administratorem.";
                return View(model);
            }
            TempData["success"] = "Plan został zaktualizowany pomyślnie.";
            return RedirectToAction(nameof(Index));
        }

        private async Task<IEnumerable<ParishMemberVM>> GetPriests(int? planId = null)
        {
            return (await _parishMemberService.GetAllInRoleAsync(Domain.Enums.Role.Priest))
                .Select(pm => new ParishMemberVM()
                {
                    Id = pm.Id,
                    DisplayName = pm.DisplayName,
                    IsActive = planId is null ? false : pm.AssignedPlans.Any(p => p.Id == planId)
                });
        }
    }
}