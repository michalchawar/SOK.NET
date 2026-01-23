using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using SOK.Application.Common.Helpers;
using SOK.Application.Services.Interface;
using SOK.Domain.Entities.Parish;
using SOK.Domain.Enums;
using SOK.Web.Filters;
using SOK.Web.ViewModels.Calendar;

namespace SOK.Web.Controllers
{
    [AuthorizeRoles(Role.Administrator, Role.Priest)]
    [RequireParish]
    [RequireActivePlan]
    [ActivePage("Calendar")]
    public class CalendarController : Controller
    {
        private readonly IPlanService _planService;
        private readonly IBuildingAssignmentService _buildingAssignmentService;
        private readonly IAgendaService _agendaService;

        public CalendarController(
            IPlanService planService, 
            IBuildingAssignmentService buildingAssignmentService,
            IAgendaService agendaService)
        {
            _planService = planService;
            _buildingAssignmentService = buildingAssignmentService;
            _agendaService = agendaService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            Plan? plan = await _planService.GetActivePlanAsync();

            if (plan == null)
            {
                return RedirectToAction(nameof(PlansController.Index), "Plans");
            }

            // Pobierz dni dla planu
            var days = await _planService.GetDaysForPlanAsync(plan.Id);

            // Jeśli nie ma dni, przekieruj do zarządzania dniami
            if (!days.Any())
            {
                return RedirectToAction(nameof(ManageDays));
            }

            // Pobierz daty z metadanych
            var visitsStartDate = await _planService.GetDateTimeMetadataAsync(plan, PlanMetadataKeys.VisitsStartDate);
            var visitsEndDate = await _planService.GetDateTimeMetadataAsync(plan, PlanMetadataKeys.VisitsEndDate);

            // Dla każdego dnia pobierz przypisane ulice pogrupowane według harmonogramów
            var dayViewModels = new List<DayItemVM>();
            foreach (var day in days)
            {
                // Pobierz budynki z przypisaniami dla tego dnia
                var buildings = await _buildingAssignmentService.GetBuildingsForDayAsync(day.Id);
                
                // Pogrupuj według harmonogramów i wyciągnij unikalne ulice
                var schedules = buildings
                    .Where(b => b.IsAssignedToThisDay)
                    .GroupBy(b => new { b.ScheduleId, b.ScheduleName, b.ScheduleColor })
                    .Select(g => new ScheduleStreetsVM
                    {
                        ScheduleId = g.Key.ScheduleId,
                        ScheduleName = g.Key.ScheduleName,
                        ScheduleColor = g.Key.ScheduleColor,
                        StreetNames = g.Select(b => b.StreetName)
                            .Distinct()
                            .OrderBy(s => s)
                            .ToList()
                    })
                    .OrderBy(s => s.ScheduleName)
                    .ToList();

                dayViewModels.Add(new DayItemVM
                {
                    Id = day.Id,
                    Date = day.Date,
                    StartHour = day.StartHour,
                    EndHour = day.EndHour,
                    Schedules = schedules
                });
            }

            var viewModel = new CalendarIndexVM
            {
                PlanId = plan.Id,
                PlanName = plan.Name,
                VisitsStartDate = visitsStartDate,
                VisitsEndDate = visitsEndDate,
                Days = dayViewModels
            };

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> ManageDays()
        {
            Plan? plan = await _planService.GetActivePlanAsync();

            if (plan == null)
            {
                return RedirectToAction(nameof(PlansController.Index), "Plans");
            }

            // Pobierz istniejące dni (tylko z bazy)
            var existingDays = await _planService.GetDaysForPlanAsync(plan.Id);

            // Pobierz daty z metadanych
            var visitsStartDate = await _planService.GetDateTimeMetadataAsync(plan, PlanMetadataKeys.VisitsStartDate) ?? DateTime.Today;
            var visitsEndDate = await _planService.GetDateTimeMetadataAsync(plan, PlanMetadataKeys.VisitsEndDate) ?? DateTime.Today.AddDays(14);

            // Pobierz domyślne godziny
            var defaultHours = new DefaultHoursVM
            {
                WeekdaysStart = await _planService.GetTimeMetadataAsync(plan, PlanMetadataKeys.DefaultStartTimeWeekdays) ?? new TimeOnly(16, 0),
                WeekdaysEnd = await _planService.GetTimeMetadataAsync(plan, PlanMetadataKeys.DefaultEndTimeWeekdays) ?? new TimeOnly(22, 0),
                SaturdayStart = await _planService.GetTimeMetadataAsync(plan, PlanMetadataKeys.DefaultStartTimeSaturday) ?? new TimeOnly(14, 0),
                SaturdayEnd = await _planService.GetTimeMetadataAsync(plan, PlanMetadataKeys.DefaultEndTimeSaturday) ?? new TimeOnly(22, 0),
                SundayStart = await _planService.GetTimeMetadataAsync(plan, PlanMetadataKeys.DefaultStartTimeSunday) ?? new TimeOnly(15, 0),
                SundayEnd = await _planService.GetTimeMetadataAsync(plan, PlanMetadataKeys.DefaultEndTimeSunday) ?? new TimeOnly(22, 0)
            };

            var viewModel = new ManageDaysVM
            {
                PlanId = plan.Id,
                PlanName = plan.Name,
                VisitsStartDate = visitsStartDate,
                VisitsEndDate = visitsEndDate,
                ExistingDays = existingDays.Select(d => new ManageDayItemVM
                {
                    Id = d.Id,
                    Date = d.Date,
                    StartHour = d.StartHour,
                    EndHour = d.EndHour
                }).ToList(),
                DefaultHours = defaultHours
            };

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> Day(int id)
        {
            var day = await _planService.GetDayAsync(id);

            if (day == null)
            {
                return NotFound();
            }

            // Pobierz wszystkie dni w planie aby znaleźć poprzedni i następny
            var allDays = await _planService.GetDaysForPlanAsync(day.PlanId);
            var sortedDays = allDays.OrderBy(d => d.Date).ToList();
            var currentIndex = sortedDays.FindIndex(d => d.Id == id);

            // Znajdź poprzedni i następny dzień
            int? previousDayId = currentIndex > 0 ? sortedDays[currentIndex - 1].Id : null;
            int? nextDayId = currentIndex < sortedDays.Count - 1 ? sortedDays[currentIndex + 1].Id : null;

            // Pobierz przypisane budynki
            var buildings = await _buildingAssignmentService.GetBuildingsForDayAsync(id);
            
            // Pogrupuj budynki według harmonogramów (tylko te przypisane do tego dnia)
            var assignedBuildings = buildings
                .Where(b => b.IsAssignedToThisDay)
                .GroupBy(b => new { b.ScheduleId, b.ScheduleName, b.ScheduleColor })
                .Select(g =>
                {
                    // Grupuj budynki według ulic
                    var buildingsByStreet = g.GroupBy(b => new { b.StreetId, b.StreetName }).ToList();
                    
                    // Znajdź ulice, które mają wszystkie budynki przypisane
                    var completeStreets = new List<CompleteStreetVM>();
                    var partialBuildings = new List<AssignedBuildingVM>();
                    
                    foreach (var street in buildingsByStreet)
                    {
                        // Pobierz wszystkie budynki z tej ulicy w tym harmonogramie
                        var allBuildingsOnStreet = buildings
                            .Where(b => b.ScheduleId == g.Key.ScheduleId && b.StreetId == street.Key.StreetId)
                            .ToList();
                        
                        var assignedBuildingsOnStreet = street.ToList();
                        
                        // Sprawdź czy wszystkie budynki z ulicy są przypisane do tego dnia
                        if (allBuildingsOnStreet.Count > 0 && 
                            allBuildingsOnStreet.All(b => b.IsAssignedToThisDay))
                        {
                            // Cała ulica jest przypisana
                            completeStreets.Add(new CompleteStreetVM
                            {
                                StreetId = street.Key.StreetId,
                                StreetName = street.Key.StreetName,
                                BuildingsCount = assignedBuildingsOnStreet.Count,
                                SubmissionsTotal = assignedBuildingsOnStreet.Sum(b => b.SubmissionsTotal),
                                SubmissionsAssignedHere = assignedBuildingsOnStreet.Sum(b => b.SubmissionsAssignedHere)
                            });
                        }
                        else
                        {
                            // Tylko część budynków z ulicy jest przypisana
                            partialBuildings.AddRange(assignedBuildingsOnStreet.Select(b => new AssignedBuildingVM
                            {
                                BuildingId = b.BuildingId,
                                StreetId = b.StreetId,
                                StreetName = b.StreetName,
                                BuildingNumber = b.BuildingNumber,
                                SubmissionsTotal = b.SubmissionsTotal,
                                SubmissionsUnassigned = b.SubmissionsUnassigned,
                                SubmissionsAssignedHere = b.SubmissionsAssignedHere
                            }));
                        }
                    }
                    
                    return new ScheduleWithBuildingsVM
                    {
                        ScheduleId = g.Key.ScheduleId,
                        ScheduleName = g.Key.ScheduleName,
                        ScheduleColor = g.Key.ScheduleColor,
                        Buildings = partialBuildings
                            .OrderBy(b => b.StreetName)
                            .ThenBy(b => int.TryParse(b.BuildingNumber, out var num) ? num : 0)
                            .ToList(),
                        CompleteStreets = completeStreets
                            .OrderBy(s => s.StreetName)
                            .ToList()
                    };
                })
                .OrderBy(s => s.ScheduleName)
                .ToList();

            var assignmentsViewModel = new DayAssignmentsVM
            {
                DayId = id,
                Schedules = assignedBuildings
            };

            // Pobierz agendy dla tego dnia
            var agendas = await _agendaService.GetAgendasForDayAsync(id);
            var agendasViewModel = new DayAgendasVM
            {
                DayId = id,
                Agendas = agendas.Select(a => new AgendaItemVM
                {
                    Id = a.Id,
                    PriestName = a.Priest?.DisplayName,
                    MinisterNames = a.Ministers.Select(m => m.DisplayName).ToList(),
                    StartHourOverride = a.StartHourOverride,
                    EndHourOverride = a.EndHourOverride,
                    VisitsCount = a.VisitsCount,
                    ShowsAssignment = !a.HideVisits,
                    ShowHours = a.ShowHours,
                    IsOfficial = a.IsOfficial,
                    GatheredFunds = a.GatheredFunds ?? 0f
                }).ToList()
            };

            ViewData["DayDate"] = day.Date.ToString("D", new CultureInfo("pl-PL"));
            ViewData["Assignments"] = assignmentsViewModel;
            ViewData["Agendas"] = agendasViewModel;
            ViewData["PreviousDayId"] = previousDayId;
            ViewData["NextDayId"] = nextDayId;
            return View(day);
        }

        [HttpGet]
        public async Task<IActionResult> AgendaEditor(int id)
        {
            var agenda = await _agendaService.GetAgendaAsync(id);

            if (agenda == null)
            {
                return NotFound();
            }

            // Pobierz dzień
            var day = await _planService.GetDayAsync(agenda.DayId);
            if (day == null)
            {
                return NotFound();
            }

            // Pobierz plan aby znać harmonogramy
            var plan = await _planService.GetPlanAsync(day.PlanId);
            if (plan == null)
            {
                return NotFound();
            }

            // Pobierz ulice
            var streets = await _buildingAssignmentService.GetBuildingsForDayAsync(agenda.DayId);
            var uniqueStreets = streets
                .Select(b => new { b.StreetId, b.StreetName })
                .Distinct()
                .OrderBy(s => s.StreetName)
                .Select(s => new { id = s.StreetId, name = s.StreetName })
                .ToList();

            ViewData["AgendaId"] = agenda.Id;
            ViewData["DayDate"] = day.Date.ToString("D", new CultureInfo("pl-PL"));
            ViewData["DayStartHour"] = day.StartHour.ToString("HH:mm");
            ViewData["DayEndHour"] = day.EndHour.ToString("HH:mm");
            ViewData["Schedules"] = plan.Schedules.Select(s => new { id = s.Id, name = s.Name }).ToList();
            ViewData["Streets"] = uniqueStreets;
            ViewData["PriestName"] = agenda.Priest?.DisplayName ?? "Brak księdza";
            ViewData["MinisterNames"] = string.Join(", ", agenda.Ministers.Select(m => m.DisplayName));

            return View(agenda);
        }
    }
}