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
using SOK.Web.Filters;
using SOK.Web.ViewModels.Parish;

namespace SOK.Web.Controllers
{
    [AllowAnonymous]
    [AllowIframe]
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
        private readonly IVisitService _visitService;
        private readonly IAgendaService _agendaService;
        private readonly IVisitTimeEstimationService _visitTimeEstimationService;

        public PublicFormController(
            ISubmissionService submissionService,
            IScheduleService scheduleService,
            IStreetService streetService,
            IParishInfoService parishInfoService,
            IBuildingService buildingService,
            ICurrentParishService currentParishService,
            IPlanService planService,
            IVisitService visitService,
            IAgendaService agendaService,
            IVisitTimeEstimationService visitTimeEstimationService)
        {
            _submissionService = submissionService;
            _scheduleService = scheduleService;
            _streetService = streetService;
            _parishInfoService = parishInfoService;
            _buildingService = buildingService;
            _currentParishService = currentParishService;
            _planService = planService;
            _visitService = visitService;
            _agendaService = agendaService;
            _visitTimeEstimationService = visitTimeEstimationService;
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
            bool isSubmissionGatheringEnabled = await _planService.IsSubmissionGatheringEnabledAsync(activePlan);
            if (!isSubmissionGatheringEnabled)
            {
                return View("FormDisabled");
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
        [IgnoreAntiforgeryToken]
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
            bool isSubmissionGatheringEnabled = await _planService.IsSubmissionGatheringEnabledAsync(activePlan);
            if (!isSubmissionGatheringEnabled)
            {
                return View("FormDisabled");
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
                    SendConfirmationEmail = model.WantsEmailNotification,
                };

                try
                {
                    int? submissionId = await _submissionService.CreateSubmissionAsync(request);
                    TempData["success"] = "Twoje zgłoszenie zostało pomyślnie utworzone.";

                    Submission submission = (await _submissionService.GetSubmissionAsync(submissionId.Value))!;
                    
                    // Przekierowanie z parishUid
                    return RedirectToAction(nameof(Success), new { parishUid, submissionUid = submission.UniqueId, submissionAccessToken = submission.AccessToken });
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
        [Route("check-auto-assign/{buildingId}")]
        public async Task<IActionResult> CheckAutoAssignment(string parishUid, int buildingId)
        {
            if (!_currentParishService.IsParishSet())
            {
                return BadRequest();
            }

            Schedule? defaultSchedule = await _scheduleService.GetDefaultScheduleAsync();
            if (defaultSchedule is null)
            {
                return Json(new { hasAutoAssign = false });
            }

            var assignmentDate = await _visitService.GetAutoAssignmentDate(buildingId, defaultSchedule.Id);

            if (assignmentDate.HasValue)
            {
                return Json(new
                {
                    hasAutoAssign = true,
                    date = assignmentDate.Value.ToString("yyyy-MM-dd"),
                    dateFormatted = assignmentDate.Value.ToString("dd.MM.yyyy")
                });
            }

            return Json(new { hasAutoAssign = false });
        }

        [HttpGet]
        [Route("success")]
        public async Task<IActionResult> Success(string parishUid, string submissionUid = "", string submissionAccessToken = "")
        {
            ViewData["ParishUid"] = parishUid;
            ViewData["SubmissionUid"] = submissionUid;
            ViewData["SubmissionAccessToken"] = submissionAccessToken;
    
            string? coreUrl = await _parishInfoService.GetValueAsync(InfoKeys.EmbeddedApplication.ControlPanelBaseUrl);
            ViewData["SubmissionPanelUrl"] = $"{coreUrl}?submissionUid={submissionUid}&accessToken={submissionAccessToken}";
            return View();
        }

        [HttpGet]
        [Route("panel/{submissionUid}")]
        public async Task<IActionResult> SubmissionPanel(string submissionUid, [FromQuery] string accessToken)
        {
            Submission? submission = await _submissionService.GetSubmissionAsync(submissionUid);
            if (submission is null || submission.AccessToken != accessToken)
            {
                return NotFound("Nie znaleziono zgłoszenia. Jeśli uważasz, że to błąd, skontaktuj się z administratorem.");
            }

            // Stwórz ViewModel i przekaż do widoku
            var viewModel = new SubmissionPanelVM();
            await PopulateSubmissionPanelVM(viewModel, submission);

            return View(viewModel);
        }

        protected async Task PopulateSubmissionPanelVM(SubmissionPanelVM vm, Submission submission)
        {
            // Dane zgłoszenia
            vm.Submission = new SubmissionInfoVM
            {
                SubmitterName = submission.Submitter.Name,
                SubmitterSurname = submission.Submitter.Surname,
                SubmitterEmail = submission.Submitter.Email,
                SubmitterPhone = submission.Submitter.Phone,
                Address = FormatAddress(submission.Address),
                SubmitterNotes = submission.SubmitterNotes,
                AdminMessage = submission.AdminMessage,
                NotesStatus = string.IsNullOrWhiteSpace(submission.SubmitterNotes) ? null : submission.NotesStatus,
                SubmitMethod = submission.FormSubmission?.Method ?? SubmitMethod.NotRegistered,
                SubmitTime = submission.SubmitTime,
                UniqueId = submission.UniqueId.ToString()
            };

            // Dane wizyty
            var estimatedTimeRange = submission.Visit.AgendaId.HasValue 
                ? await _visitTimeEstimationService.CalculateDynamicTimeRangeAsync(submission.Visit.Id)
                : null;

            // Sprawdź czy czas jest dynamiczny (agenda rozpoczęta)
            var isDynamic = false;
            if (submission.Visit.Agenda != null && estimatedTimeRange.HasValue)
            {
                var agendaStartDateTime = new DateTime(submission.Visit.Agenda.Day.Date, 
                    submission.Visit.Agenda.StartHourOverride ?? submission.Visit.Agenda.Day.StartHour);
                var polandTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Central European Standard Time");
                var now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, polandTimeZone);
                
                var visits = await _agendaService.GetAgendaVisitsAsync(submission.Visit.Agenda.Id);

                isDynamic = now >= agendaStartDateTime.AddMinutes(-15) && 
                    visits.Any(v => 
                        v.Status == VisitStatus.Visited || 
                        v.Status == VisitStatus.Rejected || 
                        v.Status == VisitStatus.Suspended || 
                        v.Status == VisitStatus.Pending);
            }

            vm.Visit = new VisitInfoVM
            {
                Status = submission.Visit.Status,
                OrdinalNumber = submission.Visit.OrdinalNumber,
                PlannedDate = submission.Visit.Agenda?.Day?.Date,
                DateVisible = !submission.Visit.Agenda?.HideVisits ?? true,
                EstimatedTime = estimatedTimeRange?.Start,
                EstimatedTimeEnd = estimatedTimeRange?.End,
                TimeVisible = submission.Visit.Agenda?.ShowHours ?? false,
                IsDynamicTimeRange = isDynamic
            };

            // Plan i harmonogram
            vm.PlanSchedule = new PlanScheduleInfoVM
            {
                PlanName = submission.Plan?.Name ?? "N/A",
                ScheduleName = submission.Visit.Schedule?.Name,
                ScheduleShortName = submission.Visit.Schedule?.ShortName
            };

            // Dane parafii
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

        private string FormatAddress(Address address)
        {
            var street = address.Building.Street;
            var streetType = street.Type?.Abbreviation ?? street.Type?.FullName ?? "";
            var streetName = street.Name;
            var buildingNumber = address.Building.Number + (address.Building.Letter ?? "");
            var apartment = address.ApartmentNumber + (address.ApartmentLetter ?? "");

            return $"{streetType} {streetName} {buildingNumber}/{apartment}";
        }

        protected async Task PopulateNewSubmissionWebFormVM(NewSubmissionWebFormVM vm)
        {
            var streets = (await _streetService.GetAllStreetsAsync(buildings: true, type: true))
                .Where(s => s.Buildings.Any(b => b.AllowSelection));

            vm.StreetList = streets.Select(s => new SelectListItem
            {
                Text = s.Type.Abbreviation + " " + s.Name,
                Value = s.Id.ToString()
            });

            vm.BuildingList = streets.ToDictionary(
                s => s.Id,
                s => s.Buildings.Where(b => b.AllowSelection)
                                .OrderBy(b => b.Number)
                                .ThenBy(b => b.Letter)
                                .Select(b => new SelectListItem
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