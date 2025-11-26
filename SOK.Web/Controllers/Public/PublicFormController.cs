using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using SOK.Application.Common.DTO;
using SOK.Application.Common.Helpers;
using SOK.Application.Services.Interface;
using SOK.Domain.Entities.Parish;
using SOK.Domain.Enums;
using SOK.Web.ViewModels.Parish;

namespace SOK.Web.Controllers
{
    [AllowAnonymous]
    [Route("{parishUid}/submissions")]
    public class PublicFormController : Controller
    {
        private readonly ISubmissionService _submissionService;
        private readonly IScheduleService _scheduleService;
        private readonly IStreetService _streetService;
        private readonly IParishInfoService _parishInfoService;
        private readonly IBuildingService _buildingService;
        private readonly ICurrentParishService _currentParishService;
        private readonly IPlanService _planService;

        public PublicFormController(
            ISubmissionService submissionService,
            IScheduleService scheduleService,
            IStreetService streetService,
            IParishInfoService parishInfoService,
            IBuildingService buildingService,
            ICurrentParishService currentParishService,
            IPlanService planService)
        {
            _submissionService = submissionService;
            _scheduleService = scheduleService;
            _streetService = streetService;
            _parishInfoService = parishInfoService;
            _buildingService = buildingService;
            _currentParishService = currentParishService;
            _planService = planService;
        }

        [HttpGet]
        [Route("")]
        [Route("new")]
        public async Task<IActionResult> New(string parishUid)
        {
            // Sprawdź czy parafia została poprawnie ustawiona przez middleware
            if (string.IsNullOrEmpty(parishUid) || !_currentParishService.IsParishSet())
            {
                return BadRequest("Nieprawidłowy identyfikator parafii.");
            }

            Plan? activePlan = await _planService.GetActivePlanAsync();
            if (activePlan is null)
            {
                return View("PlanError");
            }
            Schedule? defaultSchedule = await _scheduleService.GetDefaultScheduleAsync();
            if (defaultSchedule is null)
            {
                return View("ScheduleError");
            }

            NewSubmissionWebFormVM model = new NewSubmissionWebFormVM();
            await PopulateNewSubmissionWebFormVM(model);

            return View(model);
        }

        [HttpPost]
        [Route("")]
        [Route("new")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> New(string parishUid, NewSubmissionWebFormVM model)
        {
            // Sprawdź czy parafia została poprawnie ustawiona
            if (string.IsNullOrEmpty(parishUid) || !_currentParishService.IsParishSet())
            {
                return BadRequest("Nieprawidłowy identyfikator parafii.");
            }

            Plan? activePlan = await _planService.GetActivePlanAsync();
            if (activePlan is null)
            {
                return View("PlanError");
            }
            Schedule? defaultSchedule = await _scheduleService.GetDefaultScheduleAsync();
            if (defaultSchedule is null)
            {
                return View("ScheduleError");
            }

            // Sprawdź zgodę RODO
            if (!model.GdprConsent)
            {
                ModelState.AddModelError("GdprConsent", "Musisz wyrazić zgodę na przetwarzanie danych osobowych.");
            }

            Building? building = await _buildingService.GetBuildingAsync(model.BuildingId);

            if (building is null)
                ModelState.AddModelError("BuildingId", "Wystąpił problem z wyborem bramy. Spróbuj ponownie.");

            // Walidacja email - wymagane jeśli checkbox jest zaznaczony
            if (model.WantsEmailNotification && string.IsNullOrWhiteSpace(model.Email))
            {
                ModelState.AddModelError("Email", "Podaj adres e-mail, aby otrzymywać powiadomienia.");
            }

            if (ModelState.IsValid)
            {
                if (defaultSchedule is null)
                {
                    ModelState.AddModelError(string.Empty, "Nie można utworzyć zgłoszenia - formularz został wyłączony.");
                    await PopulateNewSubmissionWebFormVM(model);
                    return View(model);
                }

                SubmissionCreationRequestDto request = new SubmissionCreationRequestDto
                {
                    Submitter = new Submitter
                    {
                        Name = model.Name,
                        Surname = model.Surname,
                        Email = model.WantsEmailNotification ? model.Email : null,
                    },
                    Schedule = defaultSchedule,
                    Building = building!,
                    SubmitterNotes = model.HasAdditionalNotes ? model.SubmitterNotes : null,
                    ApartmentNumber = string.IsNullOrWhiteSpace(model.Apartment) ? null : int.Parse(new string(model.Apartment.TakeWhile(c => char.IsDigit(c)).ToArray())),
                    ApartmentLetter = string.IsNullOrWhiteSpace(model.Apartment) ? null : new string(model.Apartment.SkipWhile(c => char.IsDigit(c)).ToArray()),
                    Author = null, // Anonimowe zgłoszenie
                    Method = SubmitMethod.WebForm,
                    IPAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                };

                try
                {
                    await _submissionService.CreateSubmissionAsync(request);
                    TempData["success"] = "Twoje zgłoszenie zostało pomyślnie utworzone.";
                    
                    // Przekierowanie z parishUid
                    return RedirectToAction(nameof(Success), new { parishUid });
                }
                catch (InvalidOperationException)
                {
                    TempData["error"] = "Podany adres jest już zgłoszony. Jeśli uważasz, że to błąd, skontaktuj się z administratorem.";
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error creating submission: {ex.Message}");
                    ModelState.AddModelError(string.Empty, "Wystąpił błąd podczas tworzenia zgłoszenia. Spróbuj ponownie.");
                }
            }

            await PopulateNewSubmissionWebFormVM(model);
            return View(model);
        }

        [HttpGet]
        [Route("success")]
        public IActionResult Success(string parishUid)
        {
            ViewData["ParishUid"] = parishUid;
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View("Error!");
        }

        protected async Task PopulateNewSubmissionWebFormVM(NewSubmissionWebFormVM vm)
        {
            var streets = await _streetService.GetAllStreetsAsync(buildings: true, type: true);

            vm.StreetList = streets.Select(s => new SelectListItem
            {
                Text = s.Type.Abbreviation + " " + s.Name,
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
            
            if (defaultSchedule is not null)
            {
                vm.Schedule = new ScheduleVM
                {
                    Name = defaultSchedule.Name,
                    ShortName = defaultSchedule.ShortName
                };
            }

            var parishInfo = await _parishInfoService.GetValuesAsync([
                InfoKeys.Parish.ShortName,
                InfoKeys.Contact.StreetAndBuilding,
                InfoKeys.Contact.CityName,
                InfoKeys.Contact.PostalCode,
                InfoKeys.Contact.Email,
                InfoKeys.Contact.MainPhone,
                InfoKeys.Contact.SecondaryPhone,
            ]);

            vm.Parish = new ParishVM
            {
                Name = parishInfo.GetValueOrDefault(InfoKeys.Parish.ShortName) ?? string.Empty,
                StreetAndBuilding = parishInfo.GetValueOrDefault(InfoKeys.Contact.StreetAndBuilding) ?? string.Empty,
                CityName = parishInfo.GetValueOrDefault(InfoKeys.Contact.CityName) ?? string.Empty,
                PostalCode = parishInfo.GetValueOrDefault(InfoKeys.Contact.PostalCode) ?? string.Empty,
                Email = parishInfo.GetValueOrDefault(InfoKeys.Contact.Email) ?? string.Empty,
                Phone = parishInfo.GetValueOrDefault(InfoKeys.Contact.MainPhone) ?? string.Empty,
                SecondaryPhone = parishInfo.GetValueOrDefault(InfoKeys.Contact.SecondaryPhone) ?? string.Empty,
            };
        }
    }
}