using SOK.Application.Common.DTO;
using SOK.Application.Common.Interface;
using SOK.Application.Services.Interface;
using SOK.Domain.Entities.Parish;

namespace SOK.Application.Services.Implementation
{
    /// <inheritdoc />
    public class BuildingAssignmentService : IBuildingAssignmentService
    {
        private readonly IUnitOfWorkParish _uow;
        private readonly IAgendaService _agendaService;

        public BuildingAssignmentService(IUnitOfWorkParish uow, IAgendaService agendaService)
        {
            _uow = uow;
            _agendaService = agendaService;
        }

        /// <inheritdoc />
        public async Task<List<BuildingWithAssignmentInfoDto>> GetBuildingsForDayAsync(int dayId)
        {
            // Pobierz dzień z planem
            var day = await _uow.Day.GetAsync(d => d.Id == dayId, includeProperties: "Plan.Schedules");
            if (day == null)
                throw new ArgumentException("Day not found.");

            // Pobierz wszystkie budynki
            var buildings = await _uow.Building.GetAllAsync(
                includeProperties: "Street,Addresses"
            );

            // Pobierz wszystkie przypisania dla tego dnia
            var assignments = await _uow.BuildingAssignment.GetAssignmentsForDayAsync(dayId);
            var assignmentDict = assignments.ToDictionary(
                a => (a.BuildingId, a.ScheduleId),
                a => a
            );

            // Pobierz wszystkie przypisania dla innych dni (aby wiedzieć czy budynek jest przypisany gdzie indziej)
            var allAssignments = await _uow.BuildingAssignment.GetAllAsync(
                filter: a => a.DayId != dayId,
                includeProperties: "Day"
            );
            var otherDayAssignments = allAssignments
                .GroupBy(a => (a.BuildingId, a.ScheduleId))
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(a => a.Day.Date).OrderBy(d => d).ToList()
                );

            // Pobierz zgłoszenia per budynek per harmonogram
            var submissions = await _uow.Submission.GetAllAsync(
                s => s.PlanId == day.PlanId && s.Address != null,
                includeProperties: "Address,Visit.Agenda"
            );

            var submissionStats = submissions
                .GroupBy(s => new 
                { 
                    BuildingId = s.Address!.BuildingId, 
                    ScheduleId = s.Visit?.ScheduleId 
                })
                .Select(g => new
                {
                    g.Key.BuildingId,
                    g.Key.ScheduleId,
                    Total = g.Count(),
                    Unassigned = g.Count(s => s.Visit.AgendaId == null),
                    AssignedHere = g.Count(s => s.Visit.Agenda?.DayId == dayId)
                })
                .ToList();

            // Stwórz słownik tylko dla zgłoszeń z przypisanym harmonogramem
            var submissionStatsDict = submissionStats
                .Where(s => s.ScheduleId.HasValue)
                .ToDictionary(
                    s => (s.BuildingId, s.ScheduleId!.Value),
                    s => (s.Total, s.Unassigned, s.AssignedHere)
                );

            // Przygotuj rezultat
            var result = new List<BuildingWithAssignmentInfoDto>();

            foreach (var building in buildings)
            {
                foreach (var schedule in day.Plan.Schedules)
                {
                    var key = (building.Id, schedule.Id);
                    var stats = submissionStatsDict.GetValueOrDefault(key, (0, 0, 0));
                    var isAssignedToThisDay = assignmentDict.ContainsKey(key);
                    var otherDayDates = otherDayAssignments.GetValueOrDefault(key, new List<DateOnly>());

                    result.Add(new BuildingWithAssignmentInfoDto
                    {
                        BuildingId = building.Id,
                        StreetId = building.StreetId,
                        StreetName = building.Street?.Name ?? "",
                        BuildingNumber = building.Number.ToString() + (building.Letter ?? ""),
                        ScheduleId = schedule.Id,
                        ScheduleName = schedule.Name,
                        SubmissionsTotal = stats.Item1,
                        SubmissionsUnassigned = stats.Item2,
                        SubmissionsAssignedHere = stats.Item3,
                        IsAssignedToThisDay = isAssignedToThisDay,
                        IsAssignedToOtherDay = otherDayDates.Count > 0,
                        AssignedDayDates = otherDayDates
                    });
                }
            }

            return result;
        }

        /// <inheritdoc />
        public async Task AssignBuildingsToDayAsync(int dayId, int scheduleId, List<int> buildingIds, int? agendaId = null)
        {
            await using var transaction = await _uow.BeginTransactionAsync();

            // Usuń wszystkie poprzednie przypisania dla tego dnia i harmonogramu
            var existingAssignments = await _uow.BuildingAssignment.GetAllAsync(
                a => a.DayId == dayId && a.ScheduleId == scheduleId);
            foreach (var assignment in existingAssignments)
            {
                _uow.BuildingAssignment.Remove(assignment);
            }

            // Dodaj nowe przypisania
            foreach (var buildingId in buildingIds)
            {
                var assignment = new BuildingAssignment
                {
                    DayId = dayId,
                    BuildingId = buildingId,
                    ScheduleId = scheduleId
                };
                _uow.BuildingAssignment.Add(assignment);
            }

            await _uow.SaveAsync();

            // Jeśli agendaId == -1, utwórz nową agendę
            // Jeśli agendaId > 0, przypisz do istniejącej agendy
            // Jeśli agendaId == null, pomiń przypisanie
            if (agendaId.HasValue && agendaId.Value == -1)
            {
                // Utwórz nową agendę
                int newAgendaId = await CreateGenericAgendaAsync(dayId);
                await AssignUnassignedSubmissionsToAgendaAsync(dayId, scheduleId, buildingIds, newAgendaId);
            }
            else if (agendaId.HasValue && agendaId.Value > 0)
            {
                // Przypisz do istniejącej agendy
                await AssignUnassignedSubmissionsToAgendaAsync(dayId, scheduleId, buildingIds, agendaId.Value);
            }
            // Jeśli agendaId == null, nic nie rób (pomiń przypisanie)

            await transaction.CommitAsync();
        }

        /// <inheritdoc />
        public async Task<List<int>> AssignBuildingsByRangeAsync(AssignRangeDto parameters)
        {
            var assignedBuildingIds = new List<int>();

            // Pobierz wszystkie budynki na danej ulicy
            var buildings = await _uow.Building.GetAllAsync(
                b => b.StreetId == parameters.StreetId,
                includeProperties: "Street,Addresses"
            );

            // Building.Number to już int, więc nie trzeba parsować
            var buildingsWithNumbers = buildings
                .Select(b => new
                {
                    Building = b,
                    Number = (int?)b.Number
                })
                .ToList();

            IEnumerable<int> selectedBuildingIds;

            switch (parameters.Mode)
            {
                case RangeMode.WholeStreet:
                    selectedBuildingIds = buildings.Select(b => b.Id);
                    break;

                case RangeMode.NumberRange:
                    if (!parameters.RangeFrom.HasValue || !parameters.RangeTo.HasValue)
                        throw new ArgumentException("RangeFrom and RangeTo are required for NumberRange mode.");

                    selectedBuildingIds = buildingsWithNumbers
                        .Where(b => b.Number >= parameters.RangeFrom && b.Number <= parameters.RangeTo)
                        .Select(b => b.Building.Id);
                    break;

                case RangeMode.IntelligentAssignment:
                    if (!parameters.StartNumber.HasValue || !parameters.Direction.HasValue)
                        throw new ArgumentException("StartNumber and Direction are required for IntelligentAssignment mode.");

                    // Sortuj według kierunku
                    var ordered = parameters.Direction == AssignmentDirection.Ascending
                        ? buildingsWithNumbers.OrderBy(b => b.Number!.Value)
                        : buildingsWithNumbers.OrderByDescending(b => b.Number!.Value);

                    // Filtruj od numeru startowego
                    var filtered = parameters.Direction == AssignmentDirection.Ascending
                        ? ordered.Where(b => b.Number >= parameters.StartNumber)
                        : ordered.Where(b => b.Number <= parameters.StartNumber);

                    // Filtruj parzystość
                    if (parameters.ParityFilter.HasValue && parameters.ParityFilter != ParityFilter.Both)
                    {
                        if (parameters.ParityFilter == ParityFilter.Even)
                            filtered = filtered.Where(b => b.Number!.Value % 2 == 0);
                        else if (parameters.ParityFilter == ParityFilter.Odd)
                            filtered = filtered.Where(b => b.Number!.Value % 2 != 0);
                    }

                    // Pobierz zgłoszenia jeśli mamy limit
                    if (parameters.MaxSubmissions.HasValue)
                    {
                        var day = await _uow.Day.GetAsync(d => d.Id == parameters.DayId, includeProperties: "Plan");
                        var submissions = await _uow.Submission.GetAllAsync(
                            s => s.PlanId == day!.PlanId && s.Address != null,
                            includeProperties: "Address,Visit"
                        );

                        var submissionsByBuilding = submissions
                            .Where(s => s.Visit?.ScheduleId == parameters.ScheduleId)
                            .GroupBy(s => s.Address!.BuildingId)
                            .ToDictionary(g => g.Key, g => g.Count());

                        // Zbierz budynki aż do limitu zgłoszeń
                        var selected = new List<int>();
                        var totalSubmissions = 0;

                        foreach (var building in filtered)
                        {
                            var buildingSubmissions = submissionsByBuilding.GetValueOrDefault(building.Building.Id, 0);
                            if (totalSubmissions + buildingSubmissions > parameters.MaxSubmissions.Value)
                                break;

                            selected.Add(building.Building.Id);
                            totalSubmissions += buildingSubmissions;
                        }

                        selectedBuildingIds = selected;
                    }
                    else
                    {
                        selectedBuildingIds = filtered.Select(b => b.Building.Id);
                    }
                    break;

                default:
                    throw new ArgumentException("Invalid RangeMode.");
            }

            // Przypisz wybrane budynki
            var buildingIdsList = selectedBuildingIds.ToList();
            await AssignBuildingsToDayAsync(parameters.DayId, parameters.ScheduleId, buildingIdsList, parameters.AgendaId);

            return buildingIdsList;
        }

        /// <inheritdoc />
        public async Task UnassignBuildingFromDayAsync(int dayId, int buildingId, int scheduleId)
        {
            var assignment = await _uow.BuildingAssignment.GetAssignmentAsync(dayId, buildingId, scheduleId);
            if (assignment != null)
            {
                _uow.BuildingAssignment.Remove(assignment);
                await _uow.SaveAsync();
            }
        }

        /// <inheritdoc />
        public async Task<List<StreetSummaryDto>> GetStreetsSummaryForDayAsync(int dayId)
        {
            // Pobierz dzień z planem
            var day = await _uow.Day.GetAsync(d => d.Id == dayId, includeProperties: "Plan.Schedules");
            if (day == null)
                throw new ArgumentException("Day not found.");

            // Pobierz wszystkie ulice
            var streets = await _uow.Street.GetAllAsync();

            // Pobierz wszystkie budynki
            var buildings = await _uow.Building.GetAllAsync(includeProperties: "Street,Addresses");
            var buildingsByStreet = buildings.GroupBy(b => b.StreetId).ToDictionary(g => g.Key, g => g.ToList());

            // Pobierz przypisania dla tego dnia
            var assignments = await _uow.BuildingAssignment.GetAssignmentsForDayAsync(dayId);
            var assignedBuildingIds = new HashSet<int>(assignments.Select(a => a.BuildingId));

            // Pobierz zgłoszenia
            var submissions = await _uow.Submission.GetAllAsync(
                s => s.PlanId == day.PlanId && s.Address != null,
                includeProperties: "Address,Visit"
            );

            var result = new List<StreetSummaryDto>();

            foreach (var street in streets)
            {
                var streetBuildings = buildingsByStreet.GetValueOrDefault(street.Id, new List<Building>());
                var assignedBuildings = streetBuildings.Count(b => assignedBuildingIds.Contains(b.Id));

                var streetSubmissions = submissions
                    .Where(s => streetBuildings.Any(b => b.Id == s.Address!.BuildingId))
                    .ToList();

                var totalSubmissions = streetSubmissions.Count;
                var assignedSubmissions = streetSubmissions.Count(s => s.Visit != null);

                result.Add(new StreetSummaryDto
                {
                    StreetId = street.Id,
                    StreetName = street.Name,
                    TotalBuildings = streetBuildings.Count,
                    AssignedBuildings = assignedBuildings,
                    TotalSubmissions = totalSubmissions,
                    AssignedSubmissions = assignedSubmissions
                });
            }

            // Sortuj po liczbie nieprzypisanych zgłoszeń (malejąco)
            return result
                .OrderByDescending(s => s.TotalSubmissions - s.AssignedSubmissions)
                .ThenBy(s => s.StreetName)
                .ToList();
        }

        /// <summary>
        /// Tworzy nową generyczną agendę dla danego dnia.
        /// </summary>
        private async Task<int> CreateGenericAgendaAsync(int dayId)
        {
            var dto = new SaveAgendaDto
            {
                DayId = dayId,
                PriestId = null,
                MinisterIds = new List<int>(),
                StartHourOverride = null,
                EndHourOverride = null
            };

            return await _agendaService.CreateAgendaAsync(dto);
        }

        /// <summary>
        /// Przypisuje nieprzypisane zgłoszenia z wybranych budynków do agendy.
        /// </summary>
        private async Task AssignUnassignedSubmissionsToAgendaAsync(int dayId, int scheduleId, List<int> buildingIds, int agendaId)
        {
            // Pobierz dzień z planem
            var day = await _uow.Day.GetAsync(d => d.Id == dayId, includeProperties: "Plan");
            if (day == null)
                throw new ArgumentException("Day not found.");

            // Pobierz wszystkie zgłoszenia z wybranych budynków
            var submissions = await _uow.Submission.GetAllAsync(
                s => s.PlanId == day.PlanId,
                includeProperties: "Address.Building,Visit"
            );

            // Filtruj: tylko z wybranych budynków, niezapisane lub bez agendy, pasujący harmonogram
            var unassignedSubmissionIds = submissions
                .Where(s => buildingIds.Contains(s.Address.BuildingId))
                .Where(s => s.Visit.AgendaId == null)
                .Where(s => s.Visit.ScheduleId == scheduleId)
                .Select(s => s.Id)
                .ToList();

            // Jeśli są zgłoszenia do przypisania, użyj metody z AgendaService
            if (unassignedSubmissionIds.Any())
            {
                var dto = new AssignVisitsToAgendaDto
                {
                    AgendaId = agendaId,
                    SubmissionIds = unassignedSubmissionIds
                };

                await _agendaService.AssignVisitsToAgendaAsync(dto);
            }
        }
    }
}
