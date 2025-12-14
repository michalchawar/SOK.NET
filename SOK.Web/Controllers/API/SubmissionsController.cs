using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SOK.Application.Common.DTO;
using SOK.Application.Common.Helpers;
using SOK.Application.Common.Helpers.EmailTypes;
using SOK.Application.Services.Interface;
using SOK.Domain.Entities.Parish;
using SOK.Domain.Enums;
using System.Linq.Expressions;

namespace SOK.Web.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class SubmissionsController : ControllerBase
    {
        private readonly ISubmissionService _submissionService;
        private readonly IPlanService _planService;
        private readonly IScheduleService _scheduleService;
        private readonly IBuildingService _buildingService;
        private readonly IEmailService _emailService;
        private readonly IParishInfoService _parishInfoService;

        public SubmissionsController(
            ISubmissionService submissionService, 
            IPlanService planService,
            IScheduleService scheduleService,
            IBuildingService buildingService,
            IEmailService emailService,
            IParishInfoService parishInfoService)
        {
            _submissionService = submissionService;
            _planService = planService;
            _scheduleService = scheduleService;
            _buildingService = buildingService;
            _emailService = emailService;
            _parishInfoService = parishInfoService;
        }

        // GET: api/<SubmissionsController>
        [HttpGet]
        public async Task<IActionResult> Get(
            [FromQuery] string? address,
            [FromQuery] string? submitter,
            [FromQuery] string? sort,
            [FromQuery] string? order,
            [FromQuery] string? emailFilter,
            [FromQuery] string? notesFilter,
            [FromQuery] string? notesStatuses,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 5)
        {
            Plan? activePlan = await _planService.GetActivePlanAsync();

            // Pobierz zgłoszenia z filtrowaniem i sortowaniem
            var (submissions, totalCount) = await _submissionService.GetSubmissionsPaginatedWithSorting(
                CreateSubmissionFilter(
                    address ?? string.Empty,
                    submitter ?? string.Empty,
                    emailFilter ?? "off",
                    notesFilter ?? "off",
                    notesStatuses ?? "",
                    activePlan?.Id ?? -1),
                sortBy: sort ?? "time",
                order: order ?? "desc",
                page: page,
                pageSize: pageSize);

            List<SubmissionDto> result = submissions.Select(s => new SubmissionDto(s)).ToList();

            return Ok(new
            {
                submissions = result,
                totalCount = totalCount,
                page = page,
                pageSize = pageSize
            });
        }

        // GET api/<SubmissionsController>/5
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            Submission? submission = await _submissionService.GetSubmissionAsync(id);
            if (submission == null)
            {
                return NotFound(new { message = "Zgłoszenie nie zostało znalezione." });
            }

            SubmissionDto result = new SubmissionDto(submission);

            return Ok(result);
        }

        // PATCH api/<SubmissionsController>/5
        [HttpPatch("{id}")]
        public async Task<IActionResult> Patch(int id, [FromBody] PatchSubmissionDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Submission? submission = await _submissionService.GetSubmissionAsync(id);
            if (submission == null)
            {
                return NotFound(new { message = "Zgłoszenie nie zostało znalezione." });
            }

            // Przechowaj stare wartości przed zmianami
            var changes = new DataChanges();
            SnapshotOldValues(submission, changes);

            // Aktualizuj dane zgłaszającego (jeśli wysłane)
            if (dto.Name != null)
                submission.Submitter.Name = dto.Name;
            
            if (dto.Surname != null)
                submission.Submitter.Surname = dto.Surname;
            
            if (dto.Email != null)
                submission.Submitter.Email = string.IsNullOrWhiteSpace(dto.Email) ? null : dto.Email;
            
            if (dto.Phone != null)
                submission.Submitter.Phone = string.IsNullOrWhiteSpace(dto.Phone) ? null : dto.Phone;

            // Aktualizuj adres (jeśli wysłane)
            if (dto.BuildingId.HasValue)
            {
                Building? building = await _buildingService.GetBuildingAsync(dto.BuildingId.Value);
                if (building == null)
                {
                    return BadRequest(new { message = "Wybrany budynek nie istnieje." });
                }

                submission.Address.Building = building;
                submission.Address.BuildingId = building.Id;
            }

            if (dto.Apartment != null) {
                submission.Address.ApartmentNumber = string.IsNullOrWhiteSpace(dto.Apartment) ? null : int.Parse(new string(dto.Apartment.TakeWhile(c => char.IsDigit(c)).ToArray()));
                submission.Address.ApartmentLetter = string.IsNullOrWhiteSpace(dto.Apartment) ? null : new string(dto.Apartment.SkipWhile(c => char.IsDigit(c)).ToArray());
            }

            // Aktualizuj notatki (jeśli wysłane)
            if (dto.SubmitterNotes != null)
                submission.SubmitterNotes = string.IsNullOrWhiteSpace(dto.SubmitterNotes) ? null : dto.SubmitterNotes;

            if (dto.AdminMessage != null)
                submission.AdminMessage = string.IsNullOrWhiteSpace(dto.AdminMessage) ? null : dto.AdminMessage;

            if (dto.AdminNotes != null)
                submission.AdminNotes = string.IsNullOrWhiteSpace(dto.AdminNotes) ? null : dto.AdminNotes;

            // Aktualizuj statusy (jeśli wysłane)
            if (dto.NotesStatus.HasValue)
                submission.NotesStatus = dto.NotesStatus.Value;

            if (dto.SubmitMethod.HasValue && submission.FormSubmission != null)
                submission.FormSubmission.Method = dto.SubmitMethod.Value;

            // Aktualizuj harmonogram (jeśli wysłane)
            if (dto.ScheduleId.HasValue && submission.Visit != null)
            {
                // Sprawdź czy schedule istnieje
                var schedules = (await _scheduleService.GetActiveSchedules()).OrderBy(s => s.Name);
                var selectedSchedule = schedules.FirstOrDefault(s => s.Id == dto.ScheduleId);
                
                if (selectedSchedule != null)
                {
                    // Ustaw tylko ScheduleId - EF sam załaduje relację
                    submission.Visit.ScheduleId = dto.ScheduleId.Value;
                }
            }

            await _submissionService.UpdateSubmissionAsync(submission);
            
            // Zapisz nowe wartości
            SnapshotNewValues(submission, changes);

            // Wyślij email o zmianach jeśli zaznaczono i jeśli są widoczne zmiany
            if (dto.SendDataChangeEmail && changes.HasChanges() && !string.IsNullOrEmpty(submission.Submitter.Email))
            {
                try
                {
                    var baseUrl = await _parishInfoService.GetValueAsync(InfoKeys.EmbeddedApplication.ControlPanelBaseUrl);
                    if (!string.IsNullOrEmpty(baseUrl))
                    {
                        var dataChangeEmail = new DataChangeEmail(submission, baseUrl, changes);
                        await _emailService.QueueEmailAsync(dataChangeEmail, submission, forceSend: true);
                    }
                }
                catch (Exception ex)
                {
                    // Loguj błąd, ale nie przerywaj operacji
                    Console.WriteLine($"Błąd wysyłania emaila o zmianach: {ex.Message}");
                }
            }

            return Ok(new SubmissionDto(submission));
        }

        private string GetNotesStatusLabel(NotesFulfillmentStatus status)
        {
            return status switch
            {
                NotesFulfillmentStatus.NA => "Nie dotyczy",
                NotesFulfillmentStatus.Pending => "Oczekuje",
                NotesFulfillmentStatus.Rejected => "Odrzucone",
                NotesFulfillmentStatus.Accepted => "Zaakceptowane",
                _ => "N/A"
            };
        }

        private void SnapshotOldValues(Submission submission, DataChanges changes)
        {
            changes.OldName = $"{submission.Submitter.Name} {submission.Submitter.Surname}";
            changes.OldAddress = $"{submission.Address.Building?.Street.Type.FullName} {submission.Address.Building?.Street.Name} {submission.Address.Building?.Number}{(submission.Address.ApartmentNumber.HasValue ? $" / {submission.Address.ApartmentNumber}{submission.Address.ApartmentLetter}" : "")}";
            changes.OldEmail = submission.Submitter.Email ?? "";
            changes.OldSubmitterNotes = submission.SubmitterNotes ?? "";
            changes.OldNotesStatus = GetNotesStatusLabel(submission.NotesStatus);
            changes.OldAdminMessage = submission.AdminMessage ?? "";
            changes.OldSchedule = submission.Visit?.Schedule?.Name ?? "Brak";
        }

        private void SnapshotNewValues(Submission submission, DataChanges changes)
        {
            changes.NewName = $"{submission.Submitter.Name} {submission.Submitter.Surname}";
            changes.NewAddress = $"{submission.Address.Building?.Street.Type.FullName} {submission.Address.Building?.Street.Name} {submission.Address.Building?.Number}{(submission.Address.ApartmentNumber.HasValue ? $" / {submission.Address.ApartmentNumber}{submission.Address.ApartmentLetter}" : "")}";
            changes.NewEmail = submission.Submitter.Email ?? "";
            changes.NewSubmitterNotes = submission.SubmitterNotes ?? "";
            changes.NewNotesStatus = GetNotesStatusLabel(submission.NotesStatus);
            changes.NewAdminMessage = submission.AdminMessage ?? "";
            changes.NewSchedule = submission.Visit?.Schedule?.Name ?? "Brak";
        }

        // GET api/<SubmissionsController>/5/form
        [HttpGet("{id}/form")]
        public async Task<IActionResult> GetFormSubmission(int id)
        {
            Submission? submission = await _submissionService.GetSubmissionAsync(id);
            if (submission == null)
            {
                return NotFound(new { message = "Zgłoszenie nie zostało znalezione." });
            }

            if (submission.FormSubmission == null)
            {
                return NotFound(new { message = "To zgłoszenie nie ma powiązanych danych formularza." });
            }

            var form = submission.FormSubmission;
            var result = new
            {
                id = form.Id,
                name = form.Name,
                surname = form.Surname,
                email = form.Email ?? string.Empty,
                phone = form.Phone ?? string.Empty,
                submitterNotes = form.SubmitterNotes ?? string.Empty,
                scheduleName = form.ScheduleName,
                apartment = form.Apartment,
                building = form.Building,
                streetSpecifier = form.StreetSpecifier,
                street = form.Street,
                city = form.City,
                method = form.Method.ToString(),
                ip = form.IP,
                submitTime = form.SubmitTime,
                authorId = form.AuthorId
            };

            return Ok(result);
        }

        protected Expression<Func<Submission, bool>> CreateSubmissionFilter(
            string address = "",
            string submitter = "",
            string emailFilter = "off",
            string notesFilter = "off",
            string notesStatuses = "",
            int? planId = null)
        {
            // Rozbij tekst po spacji i slashu, odrzuć puste fragmenty
            static string BuildLikePattern(string input)
            {
                if (string.IsNullOrWhiteSpace(input))
                    return string.Empty;

                var parts = input
                    .ToLower()
                    .Split(new[] { ' ', '/' }, StringSplitOptions.RemoveEmptyEntries);

                // Sklejamy z wildcardami pomiędzy i na początku/końcu
                return $"%{string.Join("%", parts)}%";
            }

            var addressPattern = BuildLikePattern(address);
            var submitterPattern = BuildLikePattern(submitter);

            // Parse note statuses if notes filter is active
            var noteStatusesList = new List<SOK.Domain.Enums.NotesFulfillmentStatus>();
            if (!string.IsNullOrEmpty(notesStatuses))
            {
                var statusStrings = notesStatuses.Split(',', StringSplitOptions.RemoveEmptyEntries);
                foreach (var statusStr in statusStrings)
                {
                    if (Enum.TryParse<SOK.Domain.Enums.NotesFulfillmentStatus>(statusStr, true, out var status))
                    {
                        noteStatusesList.Add(status);
                    }
                }
            }

            // Generujemy dynamiczne wyrażenie do filtrowania i zwracamy je
            return s =>
                    (string.IsNullOrEmpty(addressPattern) ||
                        EF.Functions.Like(s.Address.FilterableString, addressPattern)) &&
                    (string.IsNullOrEmpty(submitterPattern) ||
                        EF.Functions.Like(s.Submitter.FilterableString, submitterPattern)) &&
                    (emailFilter == "off" ||
                        (emailFilter == "on" && !string.IsNullOrEmpty(s.Submitter.Email)) ||
                        (emailFilter == "reverse" && string.IsNullOrEmpty(s.Submitter.Email))) &&
                    (notesFilter == "off" ||
                        (notesFilter == "on" && (!string.IsNullOrEmpty(s.SubmitterNotes) || !string.IsNullOrEmpty(s.AdminNotes) || !string.IsNullOrEmpty(s.AdminMessage))) ||
                        (notesFilter == "reverse" && string.IsNullOrEmpty(s.SubmitterNotes) && string.IsNullOrEmpty(s.AdminNotes) && string.IsNullOrEmpty(s.AdminMessage))) &&
                    (noteStatusesList.Count == 0 || noteStatusesList.Contains(s.NotesStatus)) &&
                    (planId == null || s.PlanId == planId);
        }
    }
}
