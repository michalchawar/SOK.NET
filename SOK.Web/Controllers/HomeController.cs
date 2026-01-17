using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SOK.Application.Services.Interface;
using SOK.Domain.Enums;
using SOK.Infrastructure.Persistence.Context;
using SOK.Web.Filters;
using SOK.Web.ViewModels.Home;
using System.Diagnostics;

namespace SOK.Web.Controllers
{
    [AuthorizeRoles]
    [ActivePage("Home")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ISubmissionService _submissionService;
        private readonly IPlanService _planService;
        private readonly IAgendaService _agendaService;
        private readonly IVisitService _visitService;
        private readonly IParishMemberService _parishMemberService;

        public HomeController(
            ILogger<HomeController> logger, 
            ISubmissionService submissionService,
            IPlanService planService,
            IAgendaService agendaService,
            IVisitService visitService,
            IParishMemberService parishMemberService)
        {
            _logger = logger;
            _submissionService = submissionService;
            _planService = planService;
            _agendaService = agendaService;
            _visitService = visitService;
            _parishMemberService = parishMemberService;
        }

        public async Task<IActionResult> Index()
        {
            DateTime now = DateTime.UtcNow;
            DateTime last24h = now.AddHours(-24);
            DateOnly last30Days = DateOnly.FromDateTime(now.AddDays(-29));
            DateOnly today = DateOnly.FromDateTime(now);

            // Pobierz wszystkie zgłoszenia z metodą zgłoszenia
            var submissions = (await _submissionService.GetSubmissionsPaginated(
                    page: 1,
                    pageSize: int.MaxValue))
                .Select(s => new
                {
                    s.SubmitTime,
                    Method = s.FormSubmission?.Method ?? SubmitMethod.NotRegistered
                }).ToList();

            var stats = new SubmissionsStatsVM
            {
                TotalCount = submissions.Count,
                TotalLast24h = submissions.Count(s => s.SubmitTime >= last24h),
                WebFormCount = submissions.Count(s => s.Method == SubmitMethod.WebForm),
                WebFormLast24h = submissions.Count(s => s.Method == SubmitMethod.WebForm && s.SubmitTime >= last24h),
                OtherCount = submissions.Count(s => s.Method != SubmitMethod.WebForm),
                OtherLast24h = submissions.Count(s => s.Method != SubmitMethod.WebForm && s.SubmitTime >= last24h)
            };

            // Dane dzienne dla wykresu (ostatnie 30 dni)
            var dailyData = submissions
                .Where(s => s.SubmitTime >= last30Days.ToDateTime(TimeOnly.MinValue))
                .GroupBy(s => s.SubmitTime.Date)
                .Select(g => new DailySubmissionsVM
                {
                    Date = DateOnly.FromDateTime(g.Key),
                    WebFormCount = g.Count(s => s.Method == SubmitMethod.WebForm),
                    OtherCount = g.Count(s => s.Method != SubmitMethod.WebForm)
                })
                .OrderBy(d => d.Date)
                .ToList();

            // Uzupełnij brakujące dni zerami
            var allDays = Enumerable.Range(0, 30)
                .Select(i => last30Days.AddDays(i))
                .ToList();

            var completeData = allDays
                .Select(date => dailyData.FirstOrDefault(d => d.Date == date) ?? new DailySubmissionsVM { Date = date })
                .ToList();

            // Pobierz dane o kolędzie (tylko dla administratorów i księży)
            UpcomingDayVM? upcomingDay = null;
            List<CalendarDayVM> calendarDays = new();
            List<MinisterAgendaVM> ministerAgendas = new();

            if (User.IsInRole(nameof(Role.Administrator)) || User.IsInRole(nameof(Role.Priest)))
            {
                var activePlan = await _planService.GetActivePlanAsync();
                if (activePlan != null)
                {
                    var days = await _planService.GetDaysForPlanAsync(activePlan.Id);
                    var sortedDays = days.OrderBy(d => d.Date).ToList();

                    // Znajdź najbliższy dzień (dziś lub w przyszłości)
                    var upcomingDayEntity = sortedDays.FirstOrDefault(d => d.Date >= today);

                    if (upcomingDayEntity != null)
                    {
                        // Pobierz agendy dla tego dnia
                        var agendas = (await _agendaService.GetAgendasForDayAsync(upcomingDayEntity.Id))
                            .Where(a => a.IsOfficial) // Tylko oficjalne
                            .ToList();
                        var totalVisitsPlanned = agendas.Sum(a => a.VisitsCount);

                        var agendaCards = new List<AgendaCardVM>();
                        foreach (var agenda in agendas)
                        {
                            // Pobierz wizyty dla tej agendy aby znaleźć ulice
                            var visits = await _agendaService.GetAgendaVisitsAsync(agenda.Id);
                            
                            // Pogrupuj według harmonogramów
                            var scheduleStreets = visits
                                .Where(v => v.StreetName != null && v.ScheduleId != null)
                                .GroupBy(v => new { v.ScheduleId, v.ScheduleName, v.ScheduleColor })
                                .Select(g => new ScheduleStreetsCardVM
                                {
                                    ScheduleName = g.Key.ScheduleName ?? string.Empty,
                                    ScheduleColor = g.Key.ScheduleColor ?? string.Empty,
                                    StreetNames = g.Select(v => v.StreetName)
                                        .Distinct()
                                        .OrderBy(s => s)
                                        .ToList()
                                })
                                .OrderBy(s => s.ScheduleName)
                                .ToList();

                            agendaCards.Add(new AgendaCardVM
                            {
                                AgendaId = agenda.Id,
                                PriestName = agenda.Priest?.DisplayName,
                                MinisterNames = agenda.Ministers.Select(m => m.DisplayName).ToList(),
                                VisitsCount = agenda.VisitsCount,
                                ScheduleStreets = scheduleStreets
                            });
                        }

                        upcomingDay = new UpcomingDayVM
                        {
                            DayId = upcomingDayEntity.Id,
                            Date = upcomingDayEntity.Date,
                            StartHour = upcomingDayEntity.StartHour,
                            EndHour = upcomingDayEntity.EndHour,
                            TotalVisitsPlanned = totalVisitsPlanned,
                            Agendas = agendaCards
                        };
                    }

                    // Przygotuj listę wszystkich dni
                    foreach (var day in sortedDays)
                    {
                        var dayAgendas = (await _agendaService.GetAgendasForDayAsync(day.Id))
                            .Where(a => a.IsOfficial) // Tylko oficjalne
                            .ToList();
                        var visitsPlanned = dayAgendas.Sum(a => a.VisitsCount);
                        
                        // Policz wizyty odbyte (dla dni które minęły)
                        int visitsCompleted = 0;
                        if (day.Date < today)
                        {
                            foreach (var agenda in dayAgendas)
                            {
                                var visits = await _agendaService.GetAgendaVisitsAsync(agenda.Id);
                                visitsCompleted += visits.Count(v => v.Status == VisitStatus.Visited);
                            }
                        }

                        calendarDays.Add(new CalendarDayVM
                        {
                            DayId = day.Id,
                            Date = day.Date,
                            VisitsPlanned = visitsPlanned,
                            VisitsCompleted = visitsCompleted,
                            AgendasCount = dayAgendas.Count,
                            IsPast = day.Date < today,
                            IsUpcoming = upcomingDayEntity != null && day.Id == upcomingDayEntity.Id
                        });
                    }
                }
            }

            // Pobierz agendy dla ministranta (tylko dla roli VisitSupport)
            if (User.IsInRole(nameof(Role.VisitSupport)))
            {
                var parishMember = await _parishMemberService.GetParishMemberAsync(User);

                if (parishMember != null)
                {
                    var userId = parishMember.Id;

                    var activePlan = await _planService.GetActivePlanAsync();
                    if (activePlan != null)
                    {
                    var days = await _planService.GetDaysForPlanAsync(activePlan.Id);
                        
                        // Pobierz tylko dni przyszłe lub dzisiejsze
                        var upcomingDays = days
                            .Where(d => d.Date >= today)
                            .OrderBy(d => d.Date)
                            .Take(10) // Ostatnich 10 dni
                            .ToList();

                        foreach (var day in upcomingDays)
                        {
                            var agendas = await _agendaService.GetAgendasForDayAsync(day.Id);
                            
                            // Filtruj agendy gdzie ministrant jest przypisany
                            var ministerAgendaEntities = agendas.Where(a => 
                                a.Ministers.Any(m => m.Id == userId)).ToList();

                            foreach (var agenda in ministerAgendaEntities)
                            {
                                ministerAgendas.Add(new MinisterAgendaVM
                                {
                                    AgendaId = agenda.Id,
                                    AgendaUniqueId = agenda.UniqueId,
                                    AccessToken = agenda.AccessToken,
                                    Date = day.Date,
                                    StartHour = day.StartHour,
                                    EndHour = day.EndHour,
                                    PriestName = agenda.Priest?.DisplayName,
                                    VisitsCount = agenda.VisitsCount,
                                    ShowHours = agenda.ShowHours,
                                    IsOfficial = agenda.IsOfficial,
                                    IsPast = day.Date < today
                                });
                            }
                        }
                    }
                }
            }

            var viewModel = new DashboardVM
            {
                SubmissionsStats = stats,
                DailySubmissions = completeData,
                UpcomingDay = upcomingDay,
                AllDays = calendarDays,
                MinisterAgendas = ministerAgendas
            };

            return View(viewModel);
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}
