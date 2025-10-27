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

        public SubmissionsController(ISubmissionService submissionService)
        {
            _submissionService = submissionService;
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
            List<Submission> submissions = await _submissionService
                .GetSubmissionsPaginated(CreateSubmissionFilter(address ?? string.Empty, submitter ?? string.Empty), page, pageSize);

            List<SubmissionDto> result = submissions.Select(s => new SubmissionDto(s)).ToList();

            return Ok(result);
        }

        // GET api/<SubmissionsController>/5
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            Submission? submission = await _submissionService.GetSubmissionByIdAsync(id);
            if (submission == null)
            {
                return NotFound();
            }

            SubmissionDto result = new SubmissionDto(submission);

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
            string submitter = "")
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
                    EF.Functions.Like(s.Submitter.FilterableString, submitterPattern));
        }
    }
}
