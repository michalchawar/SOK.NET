using SOK.Application.Common.DTO;
using SOK.Application.Common.Interface;
using SOK.Application.Services.Interface;
using SOK.Domain.Entities.Parish;
using SOK.Domain.Enums;

namespace SOK.Application.Services.Implementation
{
    /// <inheritdoc />
    public class AgendaService : IAgendaService
    {
        private readonly IUnitOfWorkParish _uow;
        private readonly IParishMemberService _parishMemberService;
        private readonly IEmailNotificationService _notificationService;
        private readonly IVisitTimeEstimationService _timeEstimationService;

        public AgendaService(
            IUnitOfWorkParish uow, 
            IParishMemberService parishMemberService, 
            IEmailNotificationService notificationService,
            IVisitTimeEstimationService timeEstimationService)
        {
            _uow = uow;
            _parishMemberService = parishMemberService;
            _notificationService = notificationService;
            _timeEstimationService = timeEstimationService;
        }

        /// <inheritdoc />
        public async Task<List<AgendaDto>> GetAgendasForDayAsync(int dayId)
        {
            var agendas = await _uow.Agenda.GetAllAsync(
                filter: a => a.DayId == dayId,
                includeProperties: "AssignedMembers,Visits"
            );

            return agendas.Select(MapToDto).ToList();
        }

        /// <inheritdoc />
        public async Task<AgendaDto?> GetAgendaAsync(int agendaId)
        {
            var agenda = await _uow.Agenda.GetAsync(
                filter: a => a.Id == agendaId,
                includeProperties: "AssignedMembers,Visits"
            );

            return agenda == null ? null : MapToDto(agenda);
        }

        /// <inheritdoc />
        public async Task<int> CreateAgendaAsync(SaveAgendaDto dto)
        {
            // Weryfikacja istnienia dnia
            var day = await _uow.Day.GetAsync(d => d.Id == dto.DayId);
            if (day == null)
                throw new ArgumentException("Day not found.");

            var agenda = new Agenda
            {
                DayId = dto.DayId,
                StartHourOverride = dto.StartHourOverride,
                EndHourOverride = dto.EndHourOverride,
                HideVisits = dto.HideVisits,
                ShowHours = dto.ShowHours
            };

            // Dodaj przypisanych użytkowników
            await AssignMembersAsync(agenda, dto.PriestId, dto.MinisterIds);

            _uow.Agenda.Add(agenda);
            await _uow.SaveAsync();

            // Zapisz metadane jednostki czasowej jeśli podane
            if (dto.MinutesPerVisit.HasValue)
            {
                await SetIntMetadataAsync(agenda, Common.Helpers.AgendaMetadataKeys.MinutesPerVisit, dto.MinutesPerVisit.Value);
            }

            return agenda.Id;
        }

        /// <inheritdoc />
        public async Task UpdateAgendaAsync(SaveAgendaDto dto)
        {
            if (!dto.Id.HasValue)
                throw new ArgumentException("Agenda ID is required for update.");

            var agenda = await _uow.Agenda.GetAsync(
                filter: a => a.Id == dto.Id.Value,
                includeProperties: "AssignedMembers",
                tracked: true
            );

            if (agenda == null)
                throw new ArgumentException("Agenda not found.");

            // Aktualizuj pola
            agenda.StartHourOverride = dto.StartHourOverride;
            agenda.EndHourOverride = dto.EndHourOverride;
            agenda.HideVisits = dto.HideVisits;
            agenda.ShowHours = dto.ShowHours;

            // Przygotuj listę docelowych ID członków
            var targetMemberIds = new List<int>();
            if (dto.PriestId.HasValue)
            {
                targetMemberIds.Add(dto.PriestId.Value);
            }
            targetMemberIds.AddRange(dto.MinisterIds);

            // Usuń członków, których nie ma w nowej liście
            var membersToRemove = agenda.AssignedMembers
                .Where(m => !targetMemberIds.Contains(m.Id))
                .ToList();

            foreach (var member in membersToRemove)
            {
                agenda.AssignedMembers.Remove(member);
            }

            // Dodaj nowych członków (tylko tych, których jeszcze nie ma)
            var existingMemberIds = agenda.AssignedMembers.Select(m => m.Id).ToHashSet();
            var newMemberIds = targetMemberIds.Where(id => !existingMemberIds.Contains(id)).ToList();

            if (newMemberIds.Count > 0)
            {
                var newMembers = await _uow.ParishMember.GetAllAsync(
                    filter: m => newMemberIds.Contains(m.Id),
                    tracked: true
                );

                foreach (var member in newMembers)
                {
                    agenda.AssignedMembers.Add(member);
                }
            }

            await _uow.SaveAsync();

            // Aktualizuj metadane jednostki czasowej
            if (dto.MinutesPerVisit.HasValue)
            {
                await SetIntMetadataAsync(agenda, Common.Helpers.AgendaMetadataKeys.MinutesPerVisit, dto.MinutesPerVisit.Value);
            }
            else
            {
                // Jeśli null, usuń metadane (użyj wartości księdza)
                await DeleteMetadataAsync(agenda, Common.Helpers.AgendaMetadataKeys.MinutesPerVisit);
            }
        }

        /// <inheritdoc />
        public async Task DeleteAgendaAsync(int agendaId)
        {
            var agenda = await _uow.Agenda.GetAsync(a => a.Id == agendaId);
            if (agenda == null)
                throw new ArgumentException("Agenda not found.");

            _uow.Agenda.Remove(agenda);
            await _uow.SaveAsync();
        }

        /// <inheritdoc />
        public async Task<List<ParishMemberSimpleDto>> GetAvailablePriestsForDayAsync(int dayId)
        {
            // Pobierz dzień z planem
            var day = await _uow.Day.GetAsync(
                filter: d => d.Id == dayId,
                includeProperties: "Plan.ActivePriests"
            );

            if (day == null)
                throw new ArgumentException("Day not found.");

            // Zwróć wszystkich księży przypisanych do planu
            return day.Plan.ActivePriests
                .Select(p => new ParishMemberSimpleDto
                {
                    Id = p.Id,
                    DisplayName = p.DisplayName
                })
                .OrderBy(p => p.DisplayName)
                .ToList();
        }

        /// <inheritdoc />
        public async Task<List<ParishMemberSimpleDto>> GetAvailableMinistersAsync()
        {
            // Pobierz wszystkich użytkowników z rolą VisitSupport (ministranci/pomocnicy)
            var members = await _parishMemberService.GetAllInRoleAsync(Role.VisitSupport);

            return members
                .Select(m => new ParishMemberSimpleDto
                {
                    Id = m.Id,
                    DisplayName = m.DisplayName
                })
                .OrderBy(m => m.DisplayName)
                .ToList();
        }

        /// <summary>
        /// Przypisuje użytkowników do agendy.
        /// </summary>
        private async Task AssignMembersAsync(Agenda agenda, int? priestId, List<int> ministerIds)
        {
            // Pobierz wszystkie potrzebne ID
            var memberIds = new List<int>();
            if (priestId.HasValue)
            {
                memberIds.Add(priestId.Value);
            }
            memberIds.AddRange(ministerIds);

            if (memberIds.Count == 0)
                return;

            // Pobierz wszystkie potrzebne encje w jednym zapytaniu ze śledzeniem
            var members = await _uow.ParishMember.GetAllAsync(
                filter: m => memberIds.Contains(m.Id),
                tracked: true
            );

            // Dodaj do kolekcji
            foreach (var member in members)
            {
                agenda.AssignedMembers.Add(member);
            }
        }

        /// <summary>
        /// Mapuje encję Agenda do DTO.
        /// </summary>
        private AgendaDto MapToDto(Agenda agenda)
        {
            // Pobierz dzień z planem aby sprawdzić księży
            var day = _uow.Day.GetAsync(
                filter: d => d.Id == agenda.DayId,
                includeProperties: "Plan.ActivePriests"
            ).GetAwaiter().GetResult();

            var activePriestIds = day?.Plan.ActivePriests.Select(p => p.Id).ToHashSet() ?? new HashSet<int>();

            // Znajdź księży i ministrantów
            var priest = agenda.AssignedMembers.FirstOrDefault(m => activePriestIds.Contains(m.Id));
            var ministers = agenda.AssignedMembers.Where(m => !activePriestIds.Contains(m.Id)).ToList();
            
            // Ustal godziny (override lub domyślne z dnia)
            var startHour = agenda.StartHourOverride ?? day?.StartHour ?? TimeOnly.MinValue;
            var endHour = agenda.EndHourOverride ?? day?.EndHour ?? TimeOnly.MaxValue;

            // Pobierz metadane jednostki czasowej
            var minutesPerVisit = GetIntMetadataAsync(agenda, Common.Helpers.AgendaMetadataKeys.MinutesPerVisit)
                .GetAwaiter().GetResult();
            
            return new AgendaDto
            {
                Id = agenda.Id,
                UniqueId = agenda.UniqueId,
                AccessToken = agenda.AccessToken,
                DayId = agenda.DayId,
                PlanId = day?.PlanId ?? 0,
                Date = day?.Date ?? DateOnly.MinValue,
                StartHour = startHour,
                EndHour = endHour,
                StartHourOverride = agenda.StartHourOverride,
                EndHourOverride = agenda.EndHourOverride,
                GatheredFunds = agenda.GatheredFunds,
                HideVisits = agenda.HideVisits,
                ShowHours = agenda.ShowHours,
                VisitsCount = agenda.Visits?.Count ?? 0,
                AssignedPriestName = priest?.DisplayName,
                Priest = priest == null ? null : new ParishMemberSimpleDto
                {
                    Id = priest.Id,
                    DisplayName = priest.DisplayName
                },
                Ministers = ministers
                    .Select(m => new ParishMemberSimpleDto
                    {
                        Id = m.Id,
                        DisplayName = m.DisplayName
                    })
                    .ToList(),
                MinutesPerVisit = minutesPerVisit
            };
        }

        // === Metody edytora agendy ===

        /// <inheritdoc />
        public async Task<List<AgendaVisitDto>> GetAgendaVisitsAsync(int agendaId)
        {
            var visits = await _uow.Visit.GetAllAsync(
                filter: v => v.AgendaId == agendaId,
                includeProperties: "Submission.Address.Building.Street.Type,Submission.Submitter,Schedule",
                orderBy: v => v.OrdinalNumber ?? 0
            );

            // Oblicz czasy dla wszystkich wizyt na raz
            var visitIds = visits.Select(v => v.Id).ToList();
            var estimatedTimes = await _timeEstimationService.CalculateEstimatedTimesForAllVisitsAsync(agendaId, visitIds);

            return visits.Select(v => new AgendaVisitDto
            {
                Id = v.Id,
                SubmissionId = v.SubmissionId,
                OrdinalNumber = v.OrdinalNumber,
                Status = v.Status,
                PeopleCount = v.PeopleCount,
                BuildingId = v.Submission.Address.BuildingId,
                BuildingNumber = v.Submission.Address.Building.Number.ToString() + (v.Submission.Address.Building.Letter ?? ""),
                BuildingLetter = v.Submission.Address.Building.Letter,
                ApartmentNumber = v.Submission.Address.ApartmentNumber,
                ApartmentLetter = v.Submission.Address.ApartmentLetter,
                DeclaredPeopleCount = null, // TODO: dodaj do Submitter jeśli będzie pole
                AdminNotes = v.Submission.AdminNotes,
                StreetId = v.Submission.Address.Building.StreetId,
                StreetTypeAbbrev = v.Submission.Address.Building.Street.Type.Abbreviation ?? "",
                StreetName = v.Submission.Address.Building.Street.Name,
                SubmitterName = $"{v.Submission.Submitter.Name} {v.Submission.Submitter.Surname}",
                FloorNumber = null,
                SubmitterNotes = v.Submission.SubmitterNotes,
                ScheduleId = v.ScheduleId,
                ScheduleName = v.Schedule?.Name,
                ScheduleShortName = v.Schedule?.ShortName,
                ScheduleColor = v.Schedule?.Color,
                EstimatedTime = estimatedTimes.ContainsKey(v.Id) ? estimatedTimes[v.Id] : null
            }).ToList();
        }

        /// <inheritdoc />
        public async Task<List<BuildingWithSubmissionsDto>> GetAvailableBuildingsAsync(
            int dayId,
            int? streetId = null,
            int? scheduleId = null,
            bool onlyUnassigned = false)
        {
            // Pobierz dzień z planem
            var day = await _uow.Day.GetAsync(
                filter: d => d.Id == dayId,
                includeProperties: "Plan"
            );

            if (day == null)
                throw new ArgumentException("Day not found.");

            // Pobierz przypisania budynków dla tego dnia
            var assignments = await _uow.BuildingAssignment.GetAllAsync(
                filter: ba => ba.DayId == dayId,
                includeProperties: "Building.Street,Schedule"
            );

            var recommendedBuildings = assignments
                .Select(a => (a.BuildingId, a.ScheduleId, a.Schedule.Name))
                .ToHashSet();

            // Pobierz wszystkie zgłoszenia dla planu (bez filtrów - filtrujemy po stronie klienta)
            var submissions = await _uow.Submission.GetAllAsync(
                filter: s => s.PlanId == day.PlanId && s.Visit.Status != VisitStatus.Withdrawn,
                includeProperties: "Address.Building.Street.Type,Submitter,Visit.Agenda.Day,Visit.Schedule"
            );

            // Pogrupuj zgłoszenia według budynków
            var buildingsDict = submissions
                .GroupBy(s => new
                {
                    BuildingId = s.Address.BuildingId,
                    BuildingNumber = s.Address.Building.Number.ToString() + (s.Address.Building.Letter ?? ""),
                    StreetId = s.Address.Building.StreetId,
                    StreetSpecifier = s.Address.Building.Street.Type.Abbreviation ?? "",
                    StreetName = s.Address.Building.Street.Name
                })
                .Select(g =>
                {
                    var submissionsInBuilding = g.Select(s => new SubmissionForAgendaDto
                    {
                        Id = s.Id,
                        SubmissionId = s.Id,
                        BuildingId = g.Key.BuildingId,
                        BuildingNumber = g.Key.BuildingNumber,
                        StreetTypeAbbrev = g.Key.StreetSpecifier,
                        StreetName = g.Key.StreetName,
                        SubmitterName = $"{s.Submitter.Name} {s.Submitter.Surname}",
                        ApartmentNumber = (s.Address.ApartmentNumber?.ToString() ?? "") + (s.Address.ApartmentLetter ?? ""),
                        FloorNumber = null,
                        SubmitterNotes = s.SubmitterNotes,
                        NotesStatus = (int)s.NotesStatus,
                        AdminNotes = s.AdminNotes,
                        AdminMessage = s.AdminMessage,
                        ScheduleId = s.Visit?.ScheduleId,
                        ScheduleName = s.Visit?.Schedule?.Name,
                        IsAssigned = s.Visit?.AgendaId != null,
                        AssignedAgendaId = s.Visit?.AgendaId,
                        AssignedDate = s.Visit?.Agenda?.Day?.Date.ToDateTime(TimeOnly.MinValue),
                        AssignedStartHour = s.Visit?.Agenda?.StartHourOverride ?? s.Visit?.Agenda?.Day?.StartHour,
                        AssignedEndHour = s.Visit?.Agenda?.EndHourOverride ?? s.Visit?.Agenda?.Day?.EndHour
                    }).ToList();

                    // Sprawdź czy budynek jest polecany
                    var isRecommended = false;
                    var scheduleIdForBuilding = 0;
                    var scheduleNameForBuilding = "";

                    foreach (var (bId, sId, sName) in recommendedBuildings)
                    {
                        if (bId == g.Key.BuildingId)
                        {
                            isRecommended = true;
                            scheduleIdForBuilding = sId;
                            scheduleNameForBuilding = sName;
                            break;
                        }
                    }

                    return new BuildingWithSubmissionsDto
                    {
                        BuildingId = g.Key.BuildingId,
                        BuildingNumber = g.Key.BuildingNumber,
                        StreetId = g.Key.StreetId,
                        StreetTypeAbbrev = g.Key.StreetSpecifier,
                        StreetName = g.Key.StreetName,
                        Submissions = submissionsInBuilding,
                        IsRecommended = isRecommended,
                        ScheduleId = scheduleIdForBuilding,
                        ScheduleName = scheduleNameForBuilding
                    };
                })
                .Where(b => b.Submissions.Any()) // Tylko bramy z zgłoszeniami
                .ToList();

            return buildingsDict;
        }

        /// <inheritdoc />
        public async Task AssignVisitsToAgendaAsync(AssignVisitsToAgendaDto dto)
        {
            // Pobierz agendę
            var agenda = await _uow.Agenda.GetAsync(
                filter: a => a.Id == dto.AgendaId,
                includeProperties: "Visits.Submission.Address.Building.Street",
                tracked: true
            );
            if (agenda == null)
                throw new ArgumentException("Agenda not found.");

            // Pobierz zgłoszenia, które jeszcze nie są w tej agendzie
            // Te, które są, pomijamy, bo już mamy zapisane
            var submissions = await _uow.Submission.GetAllAsync(
                filter: s => dto.SubmissionIds
                    .Except(agenda.Visits.Select(v => v.SubmissionId))
                    .Contains(s.Id),
                includeProperties: "Visit,Address.Building.Street",
                tracked: true
            );

            var visitList = agenda.Visits.OrderBy(v => v.OrdinalNumber).ToList();

            foreach (var submission in submissions)
            {
                int insertPosition = GetAdequatePosition(submission.Visit, visitList);

                // Wstaw wizytę w odpowiednie miejsce na liście
                visitList.Insert(insertPosition, submission.Visit);
                
                // Zaktualizuj dane wizyty
                submission.Visit.AgendaId = agenda.Id;
                submission.Visit.Status = VisitStatus.Planned;

                if (dto.SendEmails)
                {
                    await _notificationService.SendVisitPlannedEmail(submission);
                }
            }

            // Zaktualizuj numery porządkowe
            for (int i = 0; i < visitList.Count; i++)
            {
                visitList[i].OrdinalNumber = i + 1;
            }

            await _uow.SaveAsync();
        }

        /// <inheritdoc />
        public async Task UpdateVisitsOrderAsync(UpdateVisitsOrderDto dto)
        {
            if (dto.Visits.Select(v => v.OrdinalNumber).Distinct().Count() != dto.Visits.Count)
                throw new ArgumentException("Duplicate ordinal numbers in the provided visits.");

            var visitIds = dto.Visits.Select(v => v.VisitId).ToList();
            var agenda = await _uow.Agenda.GetAsync(
                filter: a => a.Id == dto.AgendaId,
                includeProperties: "Visits",
                tracked: true
            );
            if (agenda == null)
                throw new ArgumentException("Agenda not found.");

            var transaction = await _uow.BeginTransactionAsync();
            // Najpierw wyczyść wszystkie OrdinalNumber aby uniknąć konfliktów z unikalnym indeksem
            foreach (var visit in agenda.Visits)
            {
                visit.OrdinalNumber = null;
            }

            await _uow.SaveAsync();

            // Teraz ustaw nowe wartości
            foreach (var visit in agenda.Visits)
            {
                var newOrder = dto.Visits.FirstOrDefault(v => v.VisitId == visit.Id);
                if (newOrder != null)
                {
                    visit.OrdinalNumber = newOrder.OrdinalNumber;
                }
                else {
                    visit.AgendaId = null;
                    visit.Status = VisitStatus.Unplanned;
                }
            }

            await _uow.SaveAsync();
            await transaction.CommitAsync();
        }

        /// <inheritdoc />
        public async Task RemoveVisitsFromAgendaAsync(RemoveVisitsFromAgendaDto dto)
        {
            var agenda = await _uow.Agenda.GetAsync(
                filter: a => a.Id == dto.AgendaId,
                includeProperties: "Visits",
                tracked: true
            );

            if (agenda == null)
                throw new ArgumentException("Agenda not found.");

            var visitIdsInOrder = agenda.Visits
                .OrderBy(v => v.OrdinalNumber)
                .Select(v => v.Id)
                .ToList();

            var transaction = await _uow.BeginTransactionAsync();
            // Najpierw wyczyść wszystkie OrdinalNumber aby uniknąć konfliktów z unikalnym indeksem
            foreach (var visit in agenda.Visits)
            {
                visit.OrdinalNumber = null;
            }

            await _uow.SaveAsync();

            int i = 1;
            foreach (var visitId in visitIdsInOrder)
            {
                var visit = agenda.Visits.FirstOrDefault(v => v.Id == visitId);
                if (visit == null) continue;

                if (!dto.VisitIds.Contains(visit.Id))
                {
                    // Wizyta zostaje, przenumeruj
                    visit.OrdinalNumber = i++;
                    continue;
                }
                
                // Usuń przypisanie wizyt
                visit.AgendaId = null;
                visit.OrdinalNumber = null;
                visit.Status = VisitStatus.Unplanned;
            }

            await _uow.SaveAsync();
            await transaction.CommitAsync();
        }
        
        /// <summary>
        /// Zwraca odpowiednią pozycję do wstawienia wizyty w danej agendzie.
        /// </summary>
        /// <returns>
        /// Numer indeksu, pod którym należy wstawić wizytę, aby pasowała do porządku.
        /// </returns>
        private int GetAdequatePosition(Visit visit, List<Visit> existingVisits)
        {
            // Jeśli brak wizyt, wstaw na początek
            if (existingVisits.Count == 0)
                return 0;

            var targetStreetId = visit.Submission.Address.Building.StreetId;
            var targetBuildingId = visit.Submission.Address.BuildingId;
            var targetBuildingNumber = visit.Submission.Address.Building.Number;
            var targetBuildingLetter = visit.Submission.Address.Building.Letter ?? "";
            var targetApartmentNumber = visit.Submission.Address.ApartmentNumber ?? 0;
            var targetApartmentLetter = visit.Submission.Address.ApartmentLetter ?? "";

            // Znajdź wszystkie wizyty na tej samej ulicy
            var visitsOnSameStreet = existingVisits
                .Where(v => v.Submission.Address.Building.StreetId == targetStreetId)
                .ToList();

            // Reguła 1: Jeśli nie ma żadnej wizyty na danej ulicy, wstaw na koniec
            if (!visitsOnSameStreet.Any())
                return existingVisits.Count;

            // Znajdź wizyty w tej samej bramie
            var visitsInSameBuilding = visitsOnSameStreet
                .Where(v => v.Submission.Address.BuildingId == targetBuildingId)
                .ToList();

            // Reguła 2: Są wizyty na ulicy, ale nie w danej bramie
            if (!visitsInSameBuilding.Any())
            {
                // Pogrupuj wizyty według bram
                var buildingGroups = visitsOnSameStreet
                    .GroupBy(v => new
                    {
                        BuildingId = v.Submission.Address.BuildingId,
                        Number = v.Submission.Address.Building.Number,
                        Letter = v.Submission.Address.Building.Letter ?? ""
                    })
                    .OrderBy(g => g.Key.Number)
                    .ThenBy(g => g.Key.Letter)
                    .ToList();

                // Reguła 2a: Tylko jedna brama na ulicy - wstaw po niej
                if (buildingGroups.Count == 1)
                {
                    var lastVisitInBuilding = visitsOnSameStreet.Last();
                    return existingVisits.IndexOf(lastVisitInBuilding) + 1;
                }

                // Reguła 2b: Wiele bram - sprawdź czy są w kolejności rosnącej lub malejącej
                var buildingNumbers = buildingGroups
                    .Select(g => (g.Key.Number, g.Key.Letter))
                    .ToList();

                bool isAscending = IsStrictlyOrdered(buildingNumbers, ascending: true);
                bool isDescending = IsStrictlyOrdered(buildingNumbers, ascending: false);

                if (isAscending || isDescending)
                {
                    // Znajdź odpowiednie miejsce w kolejności
                    for (int i = 0; i < buildingGroups.Count; i++)
                    {
                        var group = buildingGroups[i];
                        int comparison = CompareBuildingNumbers(
                            targetBuildingNumber, targetBuildingLetter,
                            group.Key.Number, group.Key.Letter
                        );

                        if ((isAscending && comparison < 0) || (isDescending && comparison > 0))
                        {
                            // Wstaw przed tą bramą
                            var firstVisitInGroup = group.First();
                            return existingVisits.IndexOf(firstVisitInGroup);
                        }
                    }

                    // Jeśli nie znaleziono miejsca wcześniej, wstaw po ostatniej bramie
                    var lastGroup = buildingGroups.Last();
                    var lastVisitInLastGroup = lastGroup.Last();
                    return existingVisits.IndexOf(lastVisitInLastGroup) + 1;
                }

                // Kolejność nie jest ściśle rosnąca ani malejąca - wstaw na koniec
                return existingVisits.Count;
            }

            // Reguła 3: Są wizyty w danej bramie
            // Znajdź wszystkie grupy wizyt w tej bramie (mogą być rozbite innymi bramami)
            var buildingVisitGroups = FindConsecutiveGroups(existingVisits, targetBuildingId);

            // Reguła 3b: Jeśli są rozbite, wybierz najbardziej liczną grupę
            var targetGroup = buildingVisitGroups.OrderByDescending(g => g.Count).First();

            // Reguła 3a: Wstaw w odpowiedniej kolejności mieszkań w wybranej grupie
            var apartmentsInGroup = targetGroup
                .Select(v => new
                {
                    Visit = v,
                    ApartmentNumber = v.Submission.Address.ApartmentNumber ?? 0,
                    ApartmentLetter = v.Submission.Address.ApartmentLetter ?? ""
                })
                .ToList();

            // Sprawdź czy mieszkania są w kolejności rosnącej lub malejącej
            var apartmentNumbers = apartmentsInGroup
                .Select(a => (a.ApartmentNumber, a.ApartmentLetter))
                .ToList();

            bool isApartmentsAscending = IsStrictlyOrdered(apartmentNumbers, ascending: true);
            bool isApartmentsDescending = IsStrictlyOrdered(apartmentNumbers, ascending: false);

            if (isApartmentsAscending || isApartmentsDescending)
            {
                // Znajdź odpowiednie miejsce w kolejności
                for (int i = 0; i < apartmentsInGroup.Count; i++)
                {
                    var apartment = apartmentsInGroup[i];
                    int comparison = CompareApartmentNumbers(
                        targetApartmentNumber, targetApartmentLetter,
                        apartment.ApartmentNumber, apartment.ApartmentLetter
                    );

                    if ((isApartmentsAscending && comparison < 0) || (isApartmentsDescending && comparison > 0))
                    {
                        // Wstaw przed tym mieszkaniem
                        return existingVisits.IndexOf(apartment.Visit);
                    }
                }
            }

            // Jeśli nie znaleziono miejsca wcześniej lub kolejność nie jest ściśle uporządkowana,
            // wybierz miejsce na podstawie kierunku bramy
            
            // Jeśli brama ma windę, idziemy od góry do dołu, tj. malejąco
            // Jeśli nie ma windy, idziemy od dołu do góry, tj. rosnąco
            var gateOrderDescending = visit.Submission.Address.Building.HasElevator;
            var gateOrderAscending = !gateOrderDescending;

            for (int i = 0; i < apartmentsInGroup.Count; i++)
            {
                var apartment = apartmentsInGroup[i];
                int comparison = CompareApartmentNumbers(
                    targetApartmentNumber, targetApartmentLetter,
                    apartment.ApartmentNumber, apartment.ApartmentLetter
                );

                if ((gateOrderAscending && comparison < 0) || (gateOrderDescending && comparison > 0))
                {
                    // Wstaw przed tym mieszkaniem
                    return existingVisits.IndexOf(apartment.Visit);
                }
            }

            // Jeśli wszystko zawiodło, wstaw na koniec grupy
            var lastVisitInGroup = targetGroup.Last();
            return existingVisits.IndexOf(lastVisitInGroup) + 1;
        }

        /// <summary>
        /// Znajduje kolejne grupy wizyt w danej bramie (które mogą być przedzielone wizytami w innych bramach).
        /// </summary>
        private List<List<Visit>> FindConsecutiveGroups(List<Visit> allVisits, int targetBuildingId)
        {
            var groups = new List<List<Visit>>();
            List<Visit>? currentGroup = null;

            foreach (var visit in allVisits)
            {
                if (visit.Submission.Address.BuildingId == targetBuildingId)
                {
                    if (currentGroup == null)
                    {
                        currentGroup = new List<Visit>();
                        groups.Add(currentGroup);
                    }
                    currentGroup.Add(visit);
                }
                else
                {
                    currentGroup = null;
                }
            }

            return groups;
        }

        /// <summary>
        /// Sprawdza czy lista numerów (z literami) jest ściśle uporządkowana rosnąco lub malejąco.
        /// </summary>
        private bool IsStrictlyOrdered(List<(int Number, string Letter)> numbers, bool ascending)
        {
            if (numbers.Count <= 1)
                return false;

            for (int i = 1; i < numbers.Count; i++)
            {
                int comparison = CompareNumbers(numbers[i - 1], numbers[i]);

                if (ascending && comparison >= 0)
                    return false;
                if (!ascending && comparison <= 0)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Porównuje dwa numery z literami.
        /// </summary>
        private int CompareNumbers((int Number, string Letter) a, (int Number, string Letter) b)
        {
            int numberComparison = a.Number.CompareTo(b.Number);
            if (numberComparison != 0)
                return numberComparison;

            return string.Compare(a.Letter, b.Letter, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Porównuje dwa numery budynków z literami.
        /// </summary>
        private int CompareBuildingNumbers(int num1, string letter1, int num2, string letter2)
        {
            int numberComparison = num1.CompareTo(num2);
            if (numberComparison != 0)
                return numberComparison;

            return string.Compare(letter1, letter2, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Porównuje dwa numery mieszkań z literami.
        /// </summary>
        private int CompareApartmentNumbers(int num1, string letter1, int num2, string letter2)
        {
            int numberComparison = num1.CompareTo(num2);
            if (numberComparison != 0)
                return numberComparison;

            return string.Compare(letter1, letter2, StringComparison.OrdinalIgnoreCase);
        }

        // === Metody przeprowadzania wizyty ===

        /// <inheritdoc />
        public async Task<AgendaDto?> GetAgendaByUniqueIdAsync(Guid uniqueId)
        {
            var agenda = await _uow.Agenda.GetAsync(
                filter: a => a.UniqueId == uniqueId,
                includeProperties: "AssignedMembers,Day,Visits"
            );

            if (agenda == null)
                return null;

            return MapToDto(agenda);
        }

        /// <inheritdoc />
        public async Task AssignPriestToAgendaAsync(int agendaId, int priestId)
        {
            var agenda = await _uow.Agenda.GetAsync(
                filter: a => a.Id == agendaId,
                includeProperties: "AssignedMembers,Day.Plan.ActivePriests",
                tracked: true
            );

            if (agenda == null)
                throw new ArgumentException("Agenda not found.");

            // Sprawdź czy ksiądz istnieje - najpierw szukaj w już załadowanych encjach
            var priest = agenda.Day.Plan.ActivePriests.FirstOrDefault(p => p.Id == priestId) 
                      ?? agenda.AssignedMembers.FirstOrDefault(m => m.Id == priestId);
            
            // Jeśli nie ma w załadowanych, pobierz bez śledzenia i sprawdź istnienie
            if (priest == null)
            {
                var priestExists = await _uow.ParishMember.GetAsync(
                    filter: pm => pm.Id == priestId,
                    tracked: false
                );
                
                if (priestExists == null)
                    throw new ArgumentException("Priest not found.");
                
                // Użyj referencji bez pełnego ładowania - EF przypisze przez Id
                priest = priestExists;
            }

            // Pobierz listę ID księży z planu
            var activePriestIds = agenda.Day.Plan.ActivePriests.Select(p => p.Id).ToHashSet();

            // Usuń poprzedniego księża jeśli był (który jest na liście ActivePriests)
            var oldPriest = agenda.AssignedMembers.FirstOrDefault(m => activePriestIds.Contains(m.Id));
            if (oldPriest != null)
            {
                agenda.AssignedMembers.Remove(oldPriest);
            }

            // Dodaj nowego księża jeśli jeszcze go nie ma
            if (!agenda.AssignedMembers.Any(m => m.Id == priestId))
            {
                agenda.AssignedMembers.Add(priest);
            }

            await _uow.SaveAsync();
        }

        /// <inheritdoc />
        public async Task UpdateGatheredFundsAsync(int agendaId, float gatheredFunds)
        {
            var agenda = await _uow.Agenda.GetAsync(
                filter: a => a.Id == agendaId,
                tracked: true
            );

            if (agenda == null)
                throw new ArgumentException("Agenda not found.");

            agenda.GatheredFunds = gatheredFunds;
            await _uow.SaveAsync();
        }

        /// <inheritdoc />
        public async Task InsertVisitToAgendaAsync(int agendaId, int submissionId)
        {
            // Pobierz agendę wraz z wizytami jako śledzone
            var agenda = await _uow.Agenda.GetAsync(
                filter: a => a.Id == agendaId,
                includeProperties: "Visits.Submission.Address.Building.Street",
                tracked: true
            );

            if (agenda == null)
                throw new ArgumentException("Agenda not found.");

            // Pobierz wizytę dla zgłoszenia wraz z pełnymi danymi adresu
            var visit = await _uow.Visit.GetAsync(
                filter: v => v.SubmissionId == submissionId,
                includeProperties: "Submission.Address.Building.Street",
                tracked: true
            );

            if (visit == null)
                throw new ArgumentException("Visit not found.");

            // Pobierz aktualną listę wizyt posortowaną według numeru porządkowego
            var visitList = agenda.Visits.OrderBy(v => v.OrdinalNumber).ToList();

            // Wyznacz odpowiednią pozycję używając tej samej logiki co przy przypisywaniu wizyt
            int insertPosition = GetAdequatePosition(visit, visitList);

            // Wstaw wizytę w odpowiednie miejsce na liście
            visitList.Insert(insertPosition, visit);

            // Zaktualizuj numery porządkowe dla wszystkich wizyt w agedzie
            for (int i = 0; i < visitList.Count; i++)
            {
                visitList[i].OrdinalNumber = i + 1;
            }

            // Przypisz wizytę do agendy
            visit.AgendaId = agendaId;
            visit.Status = VisitStatus.Planned;

            await _uow.SaveAsync();
        }

        // === Metody metadanych ===

        /// <inheritdoc />
        public async Task<int?> GetIntMetadataAsync(Agenda agenda, string metadataKey)
        {
            string? value = await _uow.ParishInfo.GetMetadataAsync(agenda, metadataKey);
            if (string.IsNullOrEmpty(value))
                return null;

            if (int.TryParse(value, out int result))
                return result;

            return null;
        }

        /// <inheritdoc />
        public async Task SetIntMetadataAsync(Agenda agenda, string metadataKey, int value)
        {
            await _uow.ParishInfo.SetMetadataAsync(agenda, metadataKey, value.ToString());
            await _uow.SaveAsync();
        }

        /// <inheritdoc />
        public async Task DeleteMetadataAsync(Agenda agenda, string metadataKey)
        {
            await _uow.ParishInfo.DeleteMetadataAsync(agenda, metadataKey);
            await _uow.SaveAsync();
        }
    }
}
