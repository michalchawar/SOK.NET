using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SOK.Application.Common.DTO;
using SOK.Application.Services.Interface;
using SOK.Domain.Entities.Parish;
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
        private readonly IBuildingService _buildingService;

        public SubmissionsController(
            ISubmissionService submissionService, 
            IPlanService planService,
            IBuildingService buildingService)
        {
            _submissionService = submissionService;
            _planService = planService;
            _buildingService = buildingService;
        }

        // GET: api/<SubmissionsController>
        [HttpGet]
        public async Task<IActionResult> Get(
            [FromQuery] string? address,
            [FromQuery] string? submitter,
            [FromQuery] string? sort,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 5)
        {
            Plan? activePlan = await _planService.GetActivePlanAsync();

            List<Submission> submissions = [.. await _submissionService
                .GetSubmissionsPaginated(
                    CreateSubmissionFilter(address ?? string.Empty, submitter ?? string.Empty, activePlan?.Id ?? -1),
                    page,
                    pageSize)];

            List<SubmissionDto> result = submissions.Select(s => new SubmissionDto(s)).ToList();

            return Ok(result);
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

            await _submissionService.UpdateSubmissionAsync(submission);

            return Ok(new SubmissionDto(submission));
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

        // POST api/<SubmissionsController>
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] string value)
        {
            return NoContent();
        }

        // PUT api/<SubmissionsController>/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] string value)
        {
            return NoContent();
        }

        // DELETE api/<SubmissionsController>/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            return NoContent();
        }

        protected Expression<Func<Submission, bool>> CreateSubmissionFilter(
            string address = "",
            string submitter = "",
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

            // Generujemy dynamiczne wyrażenie do filtrowania i zwracamy je
            return s =>
                    (string.IsNullOrEmpty(addressPattern) ||
                        EF.Functions.Like(s.Address.FilterableString, addressPattern)) &&
                    (string.IsNullOrEmpty(submitterPattern) ||
                        EF.Functions.Like(s.Submitter.FilterableString, submitterPattern)) &&
                    (planId == null || s.PlanId == planId);
        }
    }
}
