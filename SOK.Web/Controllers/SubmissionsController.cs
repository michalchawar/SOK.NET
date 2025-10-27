using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SOK.Application.Common.DTO;
using SOK.Application.Common.Interface;
using SOK.Application.Services.Interface;
using SOK.Domain.Entities.Parish;
using SOK.Web.ViewModels.Parish;

namespace SOK.Web.Controllers
{
    [Authorize]
    public class SubmissionsController : Controller
    {
        private readonly IUnitOfWorkParish _uow;
        private readonly ISubmissionService _submissionService;
        private readonly UserManager<Domain.Entities.Central.User> _userManager;

        public SubmissionsController(
            IUnitOfWorkParish uow, 
            ISubmissionService submissionService,
            UserManager<Domain.Entities.Central.User> userManager)
        {
            _uow = uow;
            _submissionService = submissionService;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            Address? randomAddress = await _uow.Address.GetRandomAsync();
            Submitter? randomSubmitter = await _uow.Submitter.GetRandomAsync();

            if (randomAddress != null)
                ViewData["SampleAddress"] = $"{randomAddress.Building.Street.Name} {randomAddress.Building.Number}{randomAddress.Building.Letter}/" +
                    $"{randomAddress.ApartmentNumber}{randomAddress.ApartmentLetter}";
            else
                ViewData["SampleAddress"] = "Brzozowa 45/2";

            if (randomSubmitter != null)
                ViewData["SampleSurname"] = randomSubmitter.Surname;
            else
                ViewData["SampleSurname"] = "Kowalski";

            return View();
        }

        [HttpGet]
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
            var building = await _uow.Building.GetAsync(b => b.Id == model.BuildingId);
            var schedule = await _uow.Schedule.GetAsync(s => s.Id == model.ScheduleId);

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
                    Author = parishMemberFound ? await _uow.ParishMember.GetAsync(pm => pm.Id == userId) : null,
                    Method = model.Method,
                    IPAddress = HttpContext.Connection.RemoteIpAddress?.ToString()
                };

                try
                {
                    await _submissionService.CreateSubmissionAsync(request);
                }
                catch (Exception ex)
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
            var streets = (await _uow.Street.GetAllAsync(includeProperties: "Buildings")).OrderBy(s => s.Name);

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

            var schedules = (await _uow.Schedule.GetAllAsync()).OrderBy(s => s.Name);
            string defaultScheduleId = await _uow.ParishInfo.GetValueAsync("DefaultScheduleId") ?? "";

            vm.ScheduleList = schedules.Select(s => new SelectListItem
            {
                Text = s.Name,
                Value = s.Id.ToString(),
                Selected = s.Id.ToString() == defaultScheduleId
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
