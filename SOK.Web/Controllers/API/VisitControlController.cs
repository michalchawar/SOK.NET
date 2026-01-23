using Microsoft.AspNetCore.Mvc;
using SOK.Application.Services.Interface;
using SOK.Domain.Enums;
using SOK.Web.Filters;
using SOK.Web.ViewModels.VisitControl;
using SOK.Web.ViewModels.Api.VisitControl;

namespace SOK.Web.Controllers.API
{
    [AuthorizeRoles(Role.VisitSupport, Role.Priest, Role.Administrator)]
    [RequireParish]
    [Route("api/[controller]")]
    [ApiController]
    public class VisitControlController : ControllerBase
    {
        private readonly IAgendaService _agendaService;
        private readonly IVisitService _visitService;
        private readonly ISubmissionService _submissionService;
        private readonly IParishMemberService _parishMemberService;

        public VisitControlController(
            IAgendaService agendaService,
            IVisitService visitService,
            ISubmissionService submissionService,
            IParishMemberService parishMemberService)
        {
            _agendaService = agendaService;
            _visitService = visitService;
            _submissionService = submissionService;
            _parishMemberService = parishMemberService;
        }

        /// <summary>
        /// Aktualizuje status wizyty.
        /// </summary>
        [HttpPost("update-status")]
        public async Task<IActionResult> UpdateVisitStatus([FromBody] UpdateVisitStatusRequest request)
        {
            try
            {
                // Walidacja dostępu
                var agenda = await _agendaService.GetAgendaByUniqueIdAsync(request.AgendaUniqueId);
                if (agenda == null || agenda.AccessToken != request.AccessToken)
                {
                    return Unauthorized(new { errors = new { access = "Nieprawidłowy dostęp." } });
                }

                await _visitService.UpdateVisitStatusAsync(
                    request.VisitId, 
                    request.Status, 
                    request.PeopleCount);

                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { errors = new { server = ex.Message } });
            }
        }

        /// <summary>
        /// Dodaje nowy adres do agendy podczas przeprowadzania wizyty.
        /// </summary>
        [HttpPost("add-address")]
        public async Task<IActionResult> AddAddress([FromBody] AddAddressRequest request)
        {
            try
            {
                // Walidacja dostępu
                var agenda = await _agendaService.GetAgendaByUniqueIdAsync(request.AgendaUniqueId);
                if (agenda == null || agenda.AccessToken != request.AccessToken)
                {
                    return Unauthorized(new { errors = new { access = "Nieprawidłowy dostęp." } });
                }

                int? apartmentNumber = string.IsNullOrWhiteSpace(request.Apartment) ? null : int.Parse(new string(request.Apartment.TakeWhile(c => char.IsDigit(c)).ToArray()));
                string? apartmentLetter = string.IsNullOrWhiteSpace(request.Apartment) ? null : new string(request.Apartment.SkipWhile(c => char.IsDigit(c)).ToArray());
                apartmentLetter = string.IsNullOrWhiteSpace(apartmentLetter) ? null : apartmentLetter;

                // Spróbuj znaleźć istniejące zgłoszenie dla tego adresu
                var existingSubmission = await _submissionService.FindSubmissionByAddressAsync(
                    request.BuildingId,
                    apartmentNumber,
                    apartmentLetter);

                int submissionId;
                bool wasCreated = false;

                if (existingSubmission != null)
                {
                    // Zgłoszenie istnieje - przepisz je do tej agendy
                    submissionId = existingSubmission.Id;

                    if (existingSubmission.Visit.Agenda?.UniqueId == request.AgendaUniqueId)
                    {
                        // Zgłoszenie już należy do tej agendy - nic więcej nie rób
                        return Ok(new 
                        { 
                            success = true,
                            wasCreated,
                            submissionId,
                            alreadyInAgenda = true
                        });
                    }
                    
                    // Dodaj adnotację do AdminNotes jeśli jeszcze jej tam nie ma
                    const string annotation = " (Przeniesiono podczas wizyty danego dnia.)";
                    if (!existingSubmission.AdminNotes?.EndsWith(annotation) ?? true)
                    {
                        await _submissionService.AppendAdminNotesAsync(
                            submissionId, 
                            annotation);
                    }
                }
                else
                {
                    // Zgłoszenia nie ma - stwórz nowe z generycznymi danymi
                    submissionId = await _submissionService.CreateSubmissionDuringVisitAsync(
                        request.BuildingId,
                        apartmentNumber,
                        apartmentLetter,
                        request.ScheduleId);
                    
                    wasCreated = true;
                }

                // Dodaj wizytę do agendy - pozycja zostanie automatycznie wyznaczona
                await _agendaService.InsertVisitToAgendaAsync(
                    agenda.Id,
                    submissionId);

                return Ok(new 
                { 
                    success = true,
                    wasCreated,
                    submissionId
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { errors = new { server = ex.Message } });
            }
        }

        /// <summary>
        /// Ustawia księża prowadzącego wizytę.
        /// </summary>
        [HttpPost("set-priest")]
        public async Task<IActionResult> SetPriest([FromBody] SetPriestRequest request)
        {
            try
            {
                // Walidacja dostępu
                var agenda = await _agendaService.GetAgendaByUniqueIdAsync(request.AgendaUniqueId);
                if (agenda == null || agenda.AccessToken != request.AccessToken)
                {
                    return Unauthorized(new { errors = new { access = "Nieprawidłowy dostęp." } });
                }

                // Sprawdź czy ksiądz istnieje
                var priest = await _parishMemberService.GetParishMemberAsync(request.PriestId);
                if (priest == null)
                {
                    return BadRequest(new { errors = new { priest = "Nie znaleziono księdza." } });
                }

                await _agendaService.AssignPriestToAgendaAsync(agenda.Id, request.PriestId);

                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { errors = new { server = ex.Message } });
            }
        }

        /// <summary>
        /// Aktualizuje zebrane fundusze.
        /// </summary>
        [HttpPost("update-funds")]
        public async Task<IActionResult> UpdateGatheredFunds([FromBody] UpdateGatheredFundsRequest request)
        {
            try
            {
                // Walidacja dostępu
                var agenda = await _agendaService.GetAgendaByUniqueIdAsync(request.AgendaUniqueId);
                if (agenda == null || agenda.AccessToken != request.AccessToken)
                {
                    return Unauthorized(new { errors = new { access = "Nieprawidłowy dostęp." } });
                }

                await _agendaService.UpdateGatheredFundsAsync(agenda.Id, request.GatheredFunds);

                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { errors = new { server = ex.Message } });
            }
        }

        /// <summary>
        /// Cofa status wizyty do Planned.
        /// </summary>
        [HttpPost("reset-status")]
        public async Task<IActionResult> ResetVisitStatus([FromBody] UpdateVisitStatusRequest request)
        {
            try
            {
                // Walidacja dostępu
                var agenda = await _agendaService.GetAgendaByUniqueIdAsync(request.AgendaUniqueId);
                if (agenda == null || agenda.AccessToken != request.AccessToken)
                {
                    return Unauthorized(new { errors = new { access = "Nieprawidłowy dostęp." } });
                }

                // Resetujemy status do Planned i usuwamy peopleCount
                await _visitService.UpdateVisitStatusAsync(
                    request.VisitId, 
                    VisitStatus.Planned, 
                    null);

                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { errors = new { server = ex.Message } });
            }
        }
    }
}
