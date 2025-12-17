using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SOK.Application.Common.DTO;
using SOK.Application.Common.Helpers;
using SOK.Application.Services.Interface;
using SOK.Domain.Entities.Parish;
using System.ComponentModel.DataAnnotations;

namespace SOK.Web.Controllers.API
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class CalendarController : ControllerBase
    {
        private readonly IPlanService _planService;
        private readonly IBuildingAssignmentService _buildingAssignmentService;
        private readonly IStreetService _streetService;

        public CalendarController(
            IPlanService planService,
            IBuildingAssignmentService buildingAssignmentService,
            IStreetService streetService)
        {
            _planService = planService;
            _buildingAssignmentService = buildingAssignmentService;
            _streetService = streetService;
        }

        /// <summary>
        /// DTO dla zapisu dni.
        /// </summary>
        public class SaveDaysRequest
        {
            [Required]
            public int PlanId { get; set; }

            [Required]
            public DateTime VisitsStartDate { get; set; }

            [Required]
            public DateTime VisitsEndDate { get; set; }

            [Required]
            public List<DayDto> Days { get; set; } = new();
        }

        public class DayDto
        {
            public int? Id { get; set; }

            [Required]
            public DateOnly Date { get; set; }

            [Required]
            public TimeOnly StartHour { get; set; }

            [Required]
            public TimeOnly EndHour { get; set; }
        }

        /// <summary>
        /// Zapisuje dni kolędowe dla planu.
        /// </summary>
        [HttpPost("days")]
        public async Task<IActionResult> SaveDays([FromBody] SaveDaysRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Plan? plan = await _planService.GetActivePlanAsync();
            if (plan == null || plan.Id != request.PlanId)
            {
                return BadRequest(new { error = "Nieprawidłowy plan." });
            }

            // Przygotuj listę dni do zapisania
            var daysToSave = request.Days
                .Select(d => new Day
                {
                    Id = d.Id ?? 0,
                    Date = d.Date,
                    StartHour = d.StartHour,
                    EndHour = d.EndHour,
                    PlanId = plan.Id
                })
                .ToList();

            try
            {
                await _planService.ManageDaysAsync(plan.Id, daysToSave, request.VisitsStartDate, request.VisitsEndDate);
                return Ok(new { success = true, message = "Dni kolędowe zostały pomyślnie zapisane." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Wystąpił błąd podczas zapisywania dni.", details = ex.Message });
            }
        }

        /// <summary>
        /// Pobiera wszystkie dane potrzebne do modalu przypisywania budynków.
        /// </summary>
        [HttpGet("modal-data")]
        public async Task<IActionResult> GetModalData([FromQuery, Required] int dayId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var day = await _planService.GetDayAsync(dayId);
                if (day == null)
                {
                    return NotFound(new { error = "Day not found." });
                }

                var plan = await _planService.GetPlanAsync(day.PlanId);
                if (plan == null)
                {
                    return NotFound(new { error = "Plan not found." });
                }

                var streets = await _streetService.GetAllStreetsAsync();

                var result = new
                {
                    dayId = day.Id,
                    date = day.Date.ToString("D", new System.Globalization.CultureInfo("pl-PL")),
                    schedules = plan.Schedules.Select(s => new
                    {
                        id = s.Id,
                        name = s.Name
                    }).ToList(),
                    streets = streets.Select(s => new
                    {
                        id = s.Id,
                        name = s.Name
                    }).OrderBy(s => s.name).ToList()
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Wystąpił błąd podczas pobierania danych.", details = ex.Message });
            }
        }

        /// <summary>
        /// Pobiera budynki wraz z informacjami o przypisaniach dla danego dnia.
        /// </summary>
        [HttpGet("day-buildings")]
        public async Task<IActionResult> GetDayBuildings([FromQuery, Required] int dayId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var buildings = await _buildingAssignmentService.GetBuildingsForDayAsync(dayId);
                return Ok(buildings);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Wystąpił błąd podczas pobierania budynków.", details = ex.Message });
            }
        }

        /// <summary>
        /// Przypisuje budynki do dnia.
        /// </summary>
        [HttpPost("assign-buildings")]
        public async Task<IActionResult> AssignBuildings([FromBody] AssignBuildingsDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                await _buildingAssignmentService.AssignBuildingsToDayAsync(
                    request.DayId,
                    request.ScheduleId,
                    request.BuildingIds,
                    request.AgendaId
                );
                return Ok(new { success = true, message = "Budynki zostały pomyślnie przypisane." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Wystąpił błąd podczas przypisywania budynków.", details = ex.Message });
            }
        }

        /// <summary>
        /// Przypisuje budynki do dnia na podstawie zakresu/kryteriów.
        /// </summary>
        [HttpPost("assign-range")]
        public async Task<IActionResult> AssignRange([FromBody] AssignRangeDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var assignedBuildingIds = await _buildingAssignmentService.AssignBuildingsByRangeAsync(request);
                return Ok(new 
                { 
                    success = true, 
                    message = "Budynki zostały pomyślnie przypisane.",
                    assignedBuildingIds 
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Wystąpił błąd podczas przypisywania budynków.", details = ex.Message });
            }
        }

        /// <summary>
        /// Usuwa przypisanie budynku z dnia.
        /// </summary>
        [HttpDelete("unassign-building")]
        public async Task<IActionResult> UnassignBuilding([FromQuery, Required] int dayId, [FromQuery, Required] int buildingId, [FromQuery, Required] int scheduleId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                await _buildingAssignmentService.UnassignBuildingFromDayAsync(dayId, buildingId, scheduleId);
                return Ok(new { success = true, message = "Przypisanie zostało pomyślnie usunięte." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Wystąpił błąd podczas usuwania przypisania.", details = ex.Message });
            }
        }

        /// <summary>
        /// Pobiera podsumowanie ulic dla danego dnia.
        /// </summary>
        [HttpGet("streets-summary")]
        public async Task<IActionResult> GetStreetsSummary([FromQuery, Required] int dayId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var summary = await _buildingAssignmentService.GetStreetsSummaryForDayAsync(dayId);
                return Ok(summary);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Wystąpił błąd podczas pobierania podsumowania.", details = ex.Message });
            }
        }
    }
}
