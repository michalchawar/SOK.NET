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

        public HomeController(ILogger<HomeController> logger, ISubmissionService submissionService)
        {
            _logger = logger;
            _submissionService = submissionService;
        }

        public async Task<IActionResult> Index()
        {
            DateTime now = DateTime.UtcNow;
            DateTime last24h = now.AddHours(-24);
            DateOnly last30Days = DateOnly.FromDateTime(now.AddDays(-29));

            // Pobierz wszystkie zgłoszenia z metodą zgłoszenia
            var submissions = (await _submissionService.GetSubmissionsPaginated(
                    page: 1,
                    pageSize: int.MaxValue))
                .Select(s => new
                {
                    s.SubmitTime,
                    s.FormSubmission.Method
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

            var viewModel = new DashboardVM
            {
                SubmissionsStats = stats,
                DailySubmissions = completeData
            };

            return View(viewModel);
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}
