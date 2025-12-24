using Microsoft.AspNetCore.Mvc;
using SOK.Application.Services.Interface;
using SOK.Domain.Enums;
using SOK.Web.Filters;
using SOK.Web.ViewModels.VisitControl;

namespace SOK.Web.Controllers
{
    [AuthorizeRoles(Role.VisitSupport, Role.Priest, Role.Administrator)]
    public class VisitControlController : Controller
    {
        private readonly IAgendaService _agendaService;
        private readonly IBuildingService _buildingService;

        public VisitControlController(
            IAgendaService agendaService,
            IBuildingService buildingService)
        {
            _agendaService = agendaService;
            _buildingService = buildingService;
        }

        /// <summary>
        /// Widok przeprowadzania wizyty kolędowej dla danej agendy.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Index(Guid agendaUniqueId, string accessToken)
        {
            // Walidacja dostępu do agendy
            var agenda = await _agendaService.GetAgendaByUniqueIdAsync(agendaUniqueId);
            
            if (agenda == null || agenda.AccessToken != accessToken)
            {
                return NotFound("Nie znaleziono agendy lub nieprawidłowy token dostępu.");
            }

            // Pobierz wizyty w agendzie
            var visits = await _agendaService.GetAgendaVisitsAsync(agenda.Id);

            // Pobierz dostępnych księży (do wyboru przy rozpoczynaniu kolędy)
            var priests = await _agendaService.GetAvailablePriestsForDayAsync(agenda.DayId);

            // Pobierz wszystkie budynki i ulice (do dodawania nowych adresów)
            var buildings = await _buildingService.GetAllBuildingsAsync();

            var viewModel = new VisitControlViewModel
            {
                AgendaId = agenda.Id,
                AgendaUniqueId = agendaUniqueId,
                AccessToken = accessToken,
                Date = agenda.Date,
                StartHour = agenda.StartHour,
                EndHour = agenda.EndHour,
                GatheredFunds = agenda.GatheredFunds,
                AssignedPriestId = agenda.Priest?.Id,
                AssignedPriestName = agenda.AssignedPriestName,
                Visits = visits.Select(v => new VisitControlItemViewModel
                {
                    VisitId = v.Id,
                    SubmissionId = v.SubmissionId,
                    Status = v.Status,
                    PeopleCount = v.PeopleCount,
                    OrdinalNumber = v.OrdinalNumber ?? 0,
                    StreetName = v.StreetName,
                    BuildingNumber = v.BuildingNumber,
                    BuildingLetter = v.BuildingLetter,
                    ApartmentNumber = v.ApartmentNumber,
                    ApartmentLetter = v.ApartmentLetter,
                    DeclaredPeopleCount = v.DeclaredPeopleCount,
                    AdminNotes = v.AdminNotes,
                    BuildingId = v.BuildingId,
                    ScheduleId = v.ScheduleId ?? visits.Where(v => v.ScheduleId != null).FirstOrDefault()?.ScheduleId ?? 0,
                    EstimatedTime = v.EstimatedTime
                }).ToList(),
                AvailablePriests = priests,
                AvailableBuildings = buildings
            };

            return View(viewModel);
        }
    }
}
