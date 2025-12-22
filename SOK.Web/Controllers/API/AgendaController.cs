using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SOK.Application.Common.DTO;
using SOK.Application.Services.Interface;
using SOK.Domain.Enums;
using SOK.Web.Filters;
using System.ComponentModel.DataAnnotations;

namespace SOK.Web.Controllers.API
{
    [AuthorizeRoles(Role.Administrator, Role.Priest)]
    [Route("api/[controller]")]
    [ApiController]
    public class AgendaController : ControllerBase
    {
        private readonly IAgendaService _agendaService;

        public AgendaController(IAgendaService agendaService)
        {
            _agendaService = agendaService;
        }

        /// <summary>
        /// Pobiera wszystkie agendy dla danego dnia.
        /// </summary>
        [HttpGet("day/{dayId}")]
        public async Task<IActionResult> GetAgendasForDay([FromRoute, Required] int dayId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var agendas = await _agendaService.GetAgendasForDayAsync(dayId);
                return Ok(agendas);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Wystąpił błąd podczas pobierania agend.", details = ex.Message });
            }
        }

        /// <summary>
        /// Pobiera dane potrzebne do modalu zarządzania agendami.
        /// </summary>
        [HttpGet("modal-data/{dayId}")]
        public async Task<IActionResult> GetModalData([FromRoute, Required] int dayId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var priests = await _agendaService.GetAvailablePriestsForDayAsync(dayId);
                var ministers = await _agendaService.GetAvailableMinistersAsync();

                var agenda = await _agendaService.GetAgendaAsync(dayId);

                return Ok(new
                {
                    priests,
                    ministers,
                    hideVisits = agenda?.HideVisits ?? false,
                    showHours = agenda?.ShowHours ?? false
                });
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Wystąpił błąd podczas pobierania danych.", details = ex.Message });
            }
        }

        /// <summary>
        /// Pobiera szczegóły agendy.
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetAgenda([FromRoute, Required] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var agenda = await _agendaService.GetAgendaAsync(id);
                if (agenda == null)
                {
                    return NotFound(new { error = "Agenda not found." });
                }
                return Ok(agenda);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Wystąpił błąd podczas pobierania agendy.", details = ex.Message });
            }
        }

        /// <summary>
        /// Tworzy nową agendę.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateAgenda([FromBody] SaveAgendaDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var agendaId = await _agendaService.CreateAgendaAsync(dto);
                return Ok(new { success = true, message = "Agenda została pomyślnie utworzona.", agendaId });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Wystąpił błąd podczas tworzenia agendy.", details = ex.Message });
            }
        }

        /// <summary>
        /// Aktualizuje istniejącą agendę.
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAgenda([FromRoute, Required] int id, [FromBody] SaveAgendaDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Ustaw ID z route
            dto.Id = id;

            try
            {
                await _agendaService.UpdateAgendaAsync(dto);
                return Ok(new { success = true, message = "Agenda została pomyślnie zaktualizowana." });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Wystąpił błąd podczas aktualizacji agendy.", details = ex.Message });
            }
        }

        /// <summary>
        /// Usuwa agendę.
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAgenda([FromRoute, Required] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                await _agendaService.DeleteAgendaAsync(id);
                return Ok(new { success = true, message = "Agenda została pomyślnie usunięta." });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Wystąpił błąd podczas usuwania agendy.", details = ex.Message });
            }
        }

        // === Endpointy edytora agendy ===

        /// <summary>
        /// Pobiera wizyty dla agendy.
        /// </summary>
        [HttpGet("{id}/visits")]
        public async Task<IActionResult> GetAgendaVisits([FromRoute, Required] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var visits = await _agendaService.GetAgendaVisitsAsync(id);
                return Ok(visits);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Wystąpił błąd podczas pobierania wizyt.", details = ex.Message });
            }
        }

        /// <summary>
        /// Pobiera dostępne bramy z zgłoszeniami dla dnia agendy.
        /// </summary>
        [HttpGet("{id}/available-buildings")]
        public async Task<IActionResult> GetAvailableBuildings(
            [FromRoute, Required] int id,
            [FromQuery] int? streetId,
            [FromQuery] int? scheduleId,
            [FromQuery] bool onlyUnassigned = false)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                // Pobierz agendę aby znać dzień
                var agenda = await _agendaService.GetAgendaAsync(id);
                if (agenda == null)
                {
                    return NotFound(new { error = "Agenda not found." });
                }

                var buildings = await _agendaService.GetAvailableBuildingsAsync(
                    agenda.DayId,
                    streetId,
                    scheduleId,
                    onlyUnassigned
                );
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
        /// Przypisuje zgłoszenia do agendy.
        /// </summary>
        [HttpPost("{id}/assign-visits")]
        public async Task<IActionResult> AssignVisits([FromRoute, Required] int id, [FromBody] AssignVisitsToAgendaDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            dto.AgendaId = id;

            try
            {
                await _agendaService.AssignVisitsToAgendaAsync(dto);
                return Ok(new { success = true, message = "Wizyty zostały pomyślnie przypisane." });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Wystąpił błąd podczas przypisywania wizyt.", details = ex.Message });
            }
        }

        /// <summary>
        /// Aktualizuje kolejność wizyt w agendzie.
        /// </summary>
        [HttpPut("{id}/visits-order")]
        public async Task<IActionResult> UpdateVisitsOrder([FromRoute, Required] int id, [FromBody] UpdateVisitsOrderDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            dto.AgendaId = id;

            try
            {
                await _agendaService.UpdateVisitsOrderAsync(dto);
                return Ok(new { success = true, message = "Kolejność wizyt została zaktualizowana." });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Wystąpił błąd podczas aktualizacji kolejności.", details = ex.Message });
            }
        }

        /// <summary>
        /// Usuwa wizyty z agendy.
        /// </summary>
        [HttpPost("{id}/remove-visits")]
        public async Task<IActionResult> RemoveVisits([FromRoute, Required] int id, [FromBody] RemoveVisitsFromAgendaDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            dto.AgendaId = id;

            try
            {
                await _agendaService.RemoveVisitsFromAgendaAsync(dto);
                return Ok(new { success = true, message = "Wizyty zostały usunięte z agendy." });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Wystąpił błąd podczas usuwania wizyt.", details = ex.Message });
            }
        }

        /// <summary>
        /// Przepisuje wizyty z innych agend do tej agendy.
        /// </summary>
        // [HttpPost("{id}/reassign-visits")]
        // public async Task<IActionResult> ReassignVisits([FromRoute, Required] int id, [FromBody] ReassignVisitsToAgendaDto dto)
        // {
        //     if (!ModelState.IsValid)
        //     {
        //         return BadRequest(ModelState);
        //     }

        //     dto.TargetAgendaId = id;

        //     try
        //     {
        //         await _agendaService.ReassignVisitsToAgendaAsync(dto);
        //         return Ok(new { success = true, message = "Wizyty zostały przepisane do agendy." });
        //     }
        //     catch (ArgumentException ex)
        //     {
        //         return BadRequest(new { error = ex.Message });
        //     }
        //     catch (Exception ex)
        //     {
        //         return StatusCode(500, new { error = "Wystąpił błąd podczas przepisywania wizyt.", details = ex.Message });
        //     }
        // }
    }
}
