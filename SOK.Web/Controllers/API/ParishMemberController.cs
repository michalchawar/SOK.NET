using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SOK.Application.Common.Helpers;
using SOK.Application.Services.Interface;
using SOK.Domain.Enums;
using SOK.Web.Filters;
using System.ComponentModel.DataAnnotations;

namespace SOK.Web.Controllers.API
{
    [AuthorizeRoles(Role.Administrator, Role.Priest)]
    [RequireParish]
    [Route("api/parish-member")]
    [ApiController]
    public class ParishMemberController : ControllerBase
    {
        private readonly IParishMemberService _parishMemberService;

        public ParishMemberController(IParishMemberService parishMemberService)
        {
            _parishMemberService = parishMemberService;
        }

        /// <summary>
        /// Pobiera jednostkę czasową (minuty na wizytę) dla księdza.
        /// </summary>
        [HttpGet("{id}/minutes-per-visit")]
        public async Task<IActionResult> GetMinutesPerVisit([FromRoute, Required] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var member = await _parishMemberService.GetParishMemberAsync(id);
                if (member == null)
                {
                    return NotFound(new { error = "Parish member not found." });
                }

                var minutesPerVisit = await _parishMemberService.GetIntMetadataAsync(
                    member, 
                    ParishMemberMetadataKeys.MinutesPerVisit
                );

                return Ok(new { minutesPerVisit = minutesPerVisit ?? 10 }); // Domyślnie 10 minut
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Wystąpił błąd podczas pobierania danych.", details = ex.Message });
            }
        }

        /// <summary>
        /// Ustawia jednostkę czasową (minuty na wizytę) dla księdza.
        /// </summary>
        [HttpPut("{id}/minutes-per-visit")]
        public async Task<IActionResult> SetMinutesPerVisit(
            [FromRoute, Required] int id, 
            [FromBody, Required] SetMinutesPerVisitDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (dto.MinutesPerVisit < 3 || dto.MinutesPerVisit > 20)
            {
                return BadRequest(new { error = "Jednostka czasowa musi być w zakresie od 3 do 20 minut." });
            }

            try
            {
                var member = await _parishMemberService.GetParishMemberAsync(id);
                if (member == null)
                {
                    return NotFound(new { error = "Parish member not found." });
                }

                await _parishMemberService.SetIntMetadataAsync(
                    member, 
                    ParishMemberMetadataKeys.MinutesPerVisit, 
                    dto.MinutesPerVisit
                );

                return Ok(new { success = true, message = "Jednostka czasowa została zaktualizowana." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Wystąpił błąd podczas zapisywania danych.", details = ex.Message });
            }
        }
    }

    /// <summary>
    /// DTO do ustawiania jednostki czasowej.
    /// </summary>
    public class SetMinutesPerVisitDto
    {
        [Required]
        [Range(3, 20)]
        public int MinutesPerVisit { get; set; }
    }
}
