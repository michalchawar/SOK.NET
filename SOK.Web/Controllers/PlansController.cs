using Microsoft.AspNetCore.Mvc;
using SOK.Application.Common.DTO.Plan;
using SOK.Application.Services.Interface;
using SOK.Domain.Entities.Parish;
using SOK.Domain.Enums;
using SOK.Web.Filters;
using SOK.Web.ViewModels.Plan;
using SOK.Web.ViewModels.ParishManagement;

namespace SOK.Web.Controllers
{
    [AuthorizeRoles(Role.Administrator, Role.Priest)]
    [RequireParish]
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
            var activePlan = await _planService.GetActivePlanAsync();

            var vm = new PlansIndexVM
            {
                Plans = plans.Select(p => new PlanListItemVM
                {
                    Id = p.Id,
                    Name = p.Name,
                    CreationTime = p.CreationTime,
                    AuthorName = p.Author?.DisplayName,
                    SubmissionsCount = p.Submissions.Count,
                    DaysCount = p.Days.Count,
                    IsActive = activePlan?.Id == p.Id
                }).ToList()
            };

            if (activePlan != null)
            {
                // Załaduj powiązane dane aktywnego planu
                var days = await _planService.GetDaysForPlanAsync(activePlan.Id);
                var users = await _parishMemberService.GetAllInRoleAsync(Role.VisitSupport);

                vm.ActivePlan = new ActivePlanVM
                {
                    Id = activePlan.Id,
                    Name = activePlan.Name,
                    CreationTime = activePlan.CreationTime,
                    Schedules = activePlan.Schedules.Select(s => new ScheduleVM
                    {
                        Id = s.Id,
                        Name = s.Name,
                        ShortName = s.ShortName,
                        Color = s.Color,
                        IsDefault = s.Id == activePlan.DefaultScheduleId
                    }).ToList(),
                    DefaultScheduleId = activePlan.DefaultScheduleId,
                    VisitsPlanned = days.Sum(d => d.Agendas.Sum(a => a.Visits.Count)),
                    VisitsSucceeded = days.Sum(d => d.Agendas.Sum(a => a.Visits.Count(v => v.Status == VisitStatus.Visited))),
                    VisitsRejected = days.Sum(d => d.Agendas.Sum(a => a.Visits.Count(v => v.Status == VisitStatus.Rejected))),
                    AgendasCount = days.Sum(d => d.Agendas.Count(a => a.IsOfficial)),
                    SupportersCount = days.Sum(d => d.Agendas.Where(a => a.IsOfficial).SelectMany(a => a.AssignedMembers)
                        .Count(m => users.Any(u => u.Id == m.Id))),
                    TotalFunds = (decimal)days.Sum(d => d.Agendas.Sum(a => a.GatheredFunds ?? 0f)),
                    IsPublicFormEnabled = await _planService.IsSubmissionGatheringEnabledAsync(activePlan)
                };
            }

            return View(vm);
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

        [HttpPost]
        public async Task<IActionResult> TogglePublicFormSending(int id)
        {
            var plan = await _planService.GetPlanAsync(id);
            if (plan == null)
            {
                TempData["error"] = "Nie znaleziono planu.";
                return RedirectToAction(nameof(Index));
            }

            bool isEnabled = await _planService.IsSubmissionGatheringEnabledAsync(plan);
            await _planService.ToggleSubmissionGatheringAsync(plan, !isEnabled);

            TempData["success"] = isEnabled
                ? "Zbieranie zgłoszeń zostało wyłączone."
                : "Zbieranie zgłoszeń zostało włączone.";
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

        [HttpGet("{id}/supporters")]
        public async Task<IActionResult> SupportersStats(int id)
        {
            var plan = await _planService.GetPlanAsync(id);
            if (plan == null)
            {
                TempData["error"] = "Nie ma aktywnego planu.";
                return RedirectToAction(nameof(Index));
            }

            var days = await _planService.GetDaysForPlanAsync(plan.Id);
            var users = await _parishMemberService.GetAllInRoleAsync(Role.VisitSupport);

            // Pobierz wszystkich ministrantów z oficjalnych agend i policz ich wizyty
            var supportersStats = days
                .SelectMany(d => d.Agendas.Where(a => a.IsOfficial))
                .SelectMany(a => a.AssignedMembers)
                .Where(m => users.Any(u => u.Id == m.Id))
                .GroupBy(m => m.Id)
                .Select(g => new SupporterStatsItemVM
                {
                    DisplayName = g.First().DisplayName,
                    VisitCount = g.Count()
                })
                .OrderByDescending(s => s.VisitCount)
                .ThenBy(s => s.DisplayName)
                .ToList();

            var vm = new SupportersStatsVM
            {
                PlanName = plan.Name,
                Supporters = supportersStats
            };

            return View(vm);
        }

        [HttpGet("{id}/stats")]
        public async Task<IActionResult> VisitStats(int id)
        {
            var stats = await _planService.GetVisitStatsAsync(id);
            if (stats == null)
            {
                TempData["error"] = "Nie znaleziono planu.";
                return RedirectToAction(nameof(Index));
            }

            var vm = new VisitStatsVM(stats);
            return View(vm);
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