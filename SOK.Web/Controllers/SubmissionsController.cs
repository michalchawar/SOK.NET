using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SOK.Application.Common.DTO;
using SOK.Application.Common.Interface;
using SOK.Application.Services.Interface;
using SOK.Domain.Entities.Parish;
using SOK.Web.Filters;
using SOK.Web.ViewModels.Parish;

namespace SOK.Web.Controllers
{
    [Authorize]
    [ActivePage("Submissions")]
    public class SubmissionsController : Controller
    {
        private readonly ISubmissionService _submissionService;
        private readonly IScheduleService _scheduleService;
        private readonly IBuildingService _buildingService;
        private readonly IParishMemberService _parishMemberService;
        private readonly IStreetService _streetService;
        private readonly UserManager<Domain.Entities.Central.User> _userManager;

        public SubmissionsController(
            ISubmissionService submissionService,
            IScheduleService scheduleService,
            IBuildingService buildingService,
            IStreetService streetService,
            IParishMemberService parishMemberService,
            UserManager<Domain.Entities.Central.User> userManager)
        {
            _submissionService = submissionService;
            _scheduleService = scheduleService;
            _buildingService = buildingService;
            _parishMemberService = parishMemberService;
            _streetService = streetService;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            Submission? randomSubmission = await _submissionService.GetRandomSubmissionAsync();

            if (randomSubmission != null)
            {
                ViewData["SampleAddress"] = $"{randomSubmission.Address.StreetName} {randomSubmission.Address.BuildingNumber}{randomSubmission.Address.BuildingLetter}/" +
                    $"{randomSubmission.Address.ApartmentNumber}{randomSubmission.Address.ApartmentLetter}";
                ViewData["SampleSurname"] = randomSubmission.Submitter.Surname;
            }
            else
            {
                ViewData["SampleAddress"] = "Brzozowa 45/2";
                ViewData["SampleSurname"] = "Kowalski";
            }

            return View();
        }

        [HttpGet]
        [ActivePage("NewSubmission")]
        public async Task<IActionResult> New()
        {
            NewSubmissionVM model = new NewSubmissionVM();
            await PopulateNewSubmissionVM(model);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> New(NewSubmissionVM model)
        {
            Building? building = await _buildingService.GetBuildingAsync(model.BuildingId);
            Schedule? schedule = await _scheduleService.GetScheduleAsync(model.ScheduleId);

            bool parishMemberFound = int.TryParse(_userManager.GetUserId(this.User), out int userId);

            if (building is null)
                ModelState.AddModelError("Building", "Wystąpił problem z wyborem bramy. Spróbuj ponownie.");
            if (schedule is null)
                ModelState.AddModelError("Schedule", "Wystąpił problem z wyborem harmonogramu. Spróbuj ponownie.");

            if (ModelState.IsValid)
            {
                SubmissionCreationRequestDto request = new SubmissionCreationRequestDto
                {
                    Submitter = new Submitter
                    {
                        Name = model.Name,
                        Surname = model.Surname,
                        Email = model.Email,
                        Phone = model.Phone,
                    },
                    Schedule = schedule!,
                    Building = building!,
                    SubmitterNotes = model.SubmitterNotes,
                    AdminNotes = model.AdminNotes,
                    ApartmentNumber = string.IsNullOrWhiteSpace(model.Apartment) ? null : int.Parse(new string(model.Apartment.TakeWhile(c => char.IsDigit(c)).ToArray())),
                    ApartmentLetter = string.IsNullOrWhiteSpace(model.Apartment) ? null : new string(model.Apartment.SkipWhile(c => char.IsDigit(c)).ToArray()),
                    Author = parishMemberFound ? await _parishMemberService.GetParishMemberAsync(userId) : null,
                    Method = model.Method,
                    IPAddress = HttpContext.Connection.RemoteIpAddress?.ToString()
                };

                try
                {
                    await _submissionService.CreateSubmissionAsync(request);
                }
                catch (Exception)
                {
                    ModelState.AddModelError(string.Empty, "Wystąpił błąd podczas tworzenia zgłoszenia. Spróbuj ponownie.");
                    await PopulateNewSubmissionVM(model);
                    return View(model);
                }

                TempData["success"] = "Twoje zgłoszenie zostało pomyślnie utworzone.";
                return RedirectToAction(nameof(New));
            }

            await PopulateNewSubmissionVM(model);
            return View(model);
        }

        protected async Task PopulateNewSubmissionVM(NewSubmissionVM vm)
        {
            var streets = await _streetService.GetAllStreetsAsync(buildings: true);

            vm.StreetList = streets.Select(s => new SelectListItem
            {
                Text = s.Name,
                Value = s.Id.ToString()
            });

            vm.BuildingList = streets.ToDictionary(
                s => s.Id,
                s => s.Buildings.OrderBy(b => b.Number).ThenBy(b => b.Letter).Select(b => new SelectListItem
                {
                    Text = b.Number + (b.Letter ?? ""),
                    Value = b.Id.ToString()
                })
            );

            var schedules = (await _scheduleService.GetActiveSchedules()).OrderBy(s => s.Name);
            Schedule? defaultSchedule = await _scheduleService.GetDefaultScheduleAsync();

            vm.ScheduleList = schedules.Select(s => new SelectListItem
            {
                Text = s.Name,
                Value = s.Id.ToString(),
                Selected = s.Id == defaultSchedule?.Id
            });
            
            if (!vm.ScheduleList.Any(sl => sl.Selected))
            {
                var firstSchedule = vm.ScheduleList.FirstOrDefault();

                if (firstSchedule is not null)
                    firstSchedule.Selected = true;
            }
        }
    }
}
