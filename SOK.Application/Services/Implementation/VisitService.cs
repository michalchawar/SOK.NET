using SOK.Application.Common.DTO.Agenda;
using SOK.Application.Common.Interface;
using SOK.Application.Services.Interface;
using SOK.Domain.Entities.Parish;
using SOK.Domain.Enums;

namespace SOK.Application.Services.Implementation
{
    /// <inheritdoc />
    public class VisitService : IVisitService
    {
        private readonly IUnitOfWorkParish _uow;
        private readonly IAgendaService _agendaService;

        public VisitService(IUnitOfWorkParish uow, IAgendaService agendaService)
        {
            _uow = uow;
            _agendaService = agendaService;
        }

        /// <inheritdoc />
        public async Task AssignVisitToDay(int visitId, int dayId, bool sendEmail = false)
        {
            // Pobierz wizytę z pełnymi danymi
            var visit = await _uow.Visit.GetAsync(
                filter: v => v.Id == visitId,
                includeProperties: "Submission.Address.Building,Schedule",
                tracked: false
            );

            if (visit == null)
                throw new ArgumentException($"Visit with ID {visitId} not found.");

            // Pobierz dzień z agendami i ich wizytami
            var day = await _uow.Day.GetAsync(
                filter: d => d.Id == dayId,
                includeProperties: "Agendas.Visits.Submission.Address.Building,Agendas.Visits.Schedule",
                tracked: false
            );

            if (day == null)
                throw new ArgumentException($"Day with ID {dayId} not found.");

            // Znajdź lub utwórz odpowiednią agendę
            int targetAgendaId = await FindOrCreateTargetAgenda(day, visit);

            // Przypisz wizytę do agendy
            var assignDto = new AssignVisitsToAgendaDto
            {
                AgendaId = targetAgendaId,
                SubmissionIds = new List<int> { visit.SubmissionId },
                SendEmails = sendEmail
            };

            await _agendaService.AssignVisitsToAgendaAsync(assignDto);
        }

        /// <summary>
        /// Znajduje lub tworzy odpowiednią agendę dla wizyty według zdefiniowanych priorytetów.
        /// </summary>
        /// <param name="day">Dzień, w którym szukamy agendy.</param>
        /// <param name="visit">Wizyta do przypisania.</param>
        /// <returns>Identyfikator wybranej lub utworzonej agendy.</returns>
        private async Task<int> FindOrCreateTargetAgenda(Day day, Visit visit)
        {
            var agendas = day.Agendas.ToList();

            // Jeśli nie ma żadnej agendy - utwórz nową
            if (agendas.Count == 0)
            {
                return await CreateDefaultAgenda(day.Id);
            }

            var buildingId = visit.Submission.Address.BuildingId;
            var scheduleId = visit.ScheduleId;

            // Priorytet 1: Agenda z największą liczbą wizyt z tego samego budynku i harmonogramu
            var agendaWithSameBuildingAndSchedule = agendas
                .Select(a => new
                {
                    Agenda = a,
                    MatchingVisitsCount = a.Visits.Count(v => 
                        v.Submission.Address.BuildingId == buildingId && 
                        v.ScheduleId == scheduleId)
                })
                .Where(x => x.MatchingVisitsCount > 0)
                .OrderByDescending(x => x.MatchingVisitsCount)
                .FirstOrDefault();

            if (agendaWithSameBuildingAndSchedule != null)
            {
                return agendaWithSameBuildingAndSchedule.Agenda.Id;
            }

            // Priorytet 2: Agenda z największą liczbą wizyt z tego samego budynku (dowolny harmonogram)
            var agendaWithSameBuilding = agendas
                .Select(a => new
                {
                    Agenda = a,
                    MatchingVisitsCount = a.Visits.Count(v => 
                        v.Submission.Address.BuildingId == buildingId)
                })
                .Where(x => x.MatchingVisitsCount > 0)
                .OrderByDescending(x => x.MatchingVisitsCount)
                .FirstOrDefault();

            if (agendaWithSameBuilding != null)
            {
                return agendaWithSameBuilding.Agenda.Id;
            }

            // Priorytet 3: Agenda z najmniejszą liczbą wizyt
            var agendaWithLeastVisits = agendas
                .OrderBy(a => a.Visits.Count)
                .First();

            return agendaWithLeastVisits.Id;
        }

        /// <summary>
        /// Tworzy domyślną agendę dla danego dnia.
        /// </summary>
        /// <param name="dayId">Identyfikator dnia.</param>
        /// <returns>Identyfikator utworzonej agendy.</returns>
        private async Task<int> CreateDefaultAgenda(int dayId)
        {
            var createDto = new SaveAgendaDto
            {
                DayId = dayId,
                PriestId = null,
                MinisterIds = new List<int>(),
                StartHourOverride = null,
                EndHourOverride = null,
                HideVisits = false,
                ShowHours = false
            };

            return await _agendaService.CreateAgendaAsync(createDto);
        }

        /// <inheritdoc />
        public async Task<DateOnly?> GetAutoAssignmentDate(int buildingId, int scheduleId)
        {
            var buildingAssignment = await _uow.BuildingAssignment.GetAsync(
                filter: ba =>
                    ba.BuildingId == buildingId &&
                    ba.ScheduleId == scheduleId &&
                    ba.EnableAutoAssign,
                includeProperties: "Day",
                tracked: false);

            return buildingAssignment?.Day?.Date;
        }

        /// <inheritdoc />
        public async Task UpdateVisitStatusAsync(int visitId, VisitStatus status, int? peopleCount)
        {
            var visit = await _uow.Visit.GetAsync(
                filter: v => v.Id == visitId,
                tracked: true
            );

            if (visit == null)
                throw new ArgumentException($"Visit with ID {visitId} not found.");

            // Jeśli ustawiamy nowy status Pending (trwająca), przenieś poprzedni Pending na Visited
            if (status == VisitStatus.Pending && visit.AgendaId.HasValue)
            {
                var previousPending = await _uow.Visit.GetAsync(
                    filter: v => v.AgendaId == visit.AgendaId && v.Status == VisitStatus.Pending && v.Id != visitId,
                    tracked: true
                );

                if (previousPending != null)
                {
                    previousPending.Status = VisitStatus.Visited;
                }
            }

            visit.Status = status;

            if (peopleCount.HasValue)
                visit.PeopleCount = peopleCount.Value;

            await _uow.SaveAsync();
        }
    }
}
