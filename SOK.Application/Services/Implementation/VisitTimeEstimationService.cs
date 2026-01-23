using Microsoft.Extensions.Logging;
using SOK.Application.Common.Helpers;
using SOK.Application.Common.Interface;
using SOK.Application.Services.Interface;
using SOK.Domain.Entities.Parish;
using SOK.Domain.Enums;

namespace SOK.Application.Services.Implementation
{
    /// <inheritdoc />
    public class VisitTimeEstimationService : IVisitTimeEstimationService
    {
        private readonly IUnitOfWorkParish _uow;
        private readonly IParishMemberService _parishMemberService;
        private readonly ILogger<VisitTimeEstimationService> _logger;

        private const int DEFAULT_MINUTES_PER_VISIT = 10;
        private const int MAX_RANGE_MINUTES = 120; // 2 godziny
        private const int MIN_RANGE_MINUTES = 15;  // 15 minut

        public VisitTimeEstimationService(
            IUnitOfWorkParish uow, 
            IParishMemberService parishMemberService,
            ILogger<VisitTimeEstimationService> logger)
        {
            _uow = uow;
            _parishMemberService = parishMemberService;
            _logger = logger;
        }

        /// <inheritdoc />
        public async Task<int> GetMinutesPerVisitForAgendaAsync(int agendaId)
        {
            // Pobierz agendę z przypisanymi członkami
            var agenda = await _uow.Agenda.GetAsync(
                filter: a => a.Id == agendaId,
                includeProperties: "AssignedMembers,Day.Plan.ActivePriests"
            );

            if (agenda == null)
                return DEFAULT_MINUTES_PER_VISIT;

            // Sprawdź, czy agenda ma nadpisaną wartość
            var agendaMetadataValue = await _uow.ParishInfo.GetMetadataAsync(agenda, AgendaMetadataKeys.MinutesPerVisit);
            if (!string.IsNullOrEmpty(agendaMetadataValue) && int.TryParse(agendaMetadataValue, out int agendaMinutes))
                return agendaMinutes;

            // Znajdź księdza przypisanego do agendy
            var activePriestIds = agenda.Day.Plan.ActivePriests.Select(p => p.Id).ToHashSet();
            var priest = agenda.AssignedMembers.FirstOrDefault(m => activePriestIds.Contains(m.Id));

            if (priest != null)
            {
                // Sprawdź wartość dla księdza
                var priestMinutes = await _parishMemberService.GetIntMetadataAsync(priest, ParishMemberMetadataKeys.MinutesPerVisit);
                if (priestMinutes.HasValue)
                    return priestMinutes.Value;
            }

            // Zwróć wartość domyślną
            return DEFAULT_MINUTES_PER_VISIT;
        }

        /// <inheritdoc />
        public async Task<TimeOnly?> CalculateEstimatedTimeAsync(int submissionId)
        {
            // Pobierz wizytę na podstawie submissionId
            var visit = await _uow.Visit.GetAsync(
                filter: v => v.SubmissionId == submissionId,
                includeProperties: "Agenda"
            );

            if (visit == null || visit.Agenda == null)
                return null;

            var agendaId = visit.Agenda.Id;
            var visitId = visit.Id;
            return await CalculateEstimatedTimeAsync(agendaId, visitId);
        }

        public async Task<TimeOnly?> CalculateEstimatedTimeAsync(int agendaId, int visitId)
        {
            // Pobierz agendę z wizytami
            var agenda = await _uow.Agenda.GetAsync(
                filter: a => a.Id == agendaId,
                includeProperties: "Visits,Day"
            );

            if (agenda == null)
                return null;

            // Znajdź pozycję wizyty
            var visits = agenda.Visits.OrderBy(v => v.OrdinalNumber).ToList();
            var visitIndex = visits.FindIndex(v => v.Id == visitId);

            if (visitIndex < 0)
                return null;

            // Pobierz jednostkę czasową
            var minutesPerVisit = await GetMinutesPerVisitForAgendaAsync(agendaId);

            // Oblicz godzinę rozpoczęcia agendy
            var startHour = agenda.StartHourOverride ?? agenda.Day.StartHour;

            // Oblicz przewidywaną godzinę: start + (pozycja * jednostka czasowa)
            var estimatedMinutes = visitIndex * minutesPerVisit;
            var estimatedTime = startHour.AddMinutes(estimatedMinutes);

            // Upewnij się, że nie przekraczamy godziny zakończenia
            var endHour = agenda.EndHourOverride ?? agenda.Day.EndHour;
            if (estimatedTime > endHour)
                estimatedTime = endHour;

            return estimatedTime;
        }

        /// <inheritdoc />
        public async Task<Dictionary<int, TimeOnly>> CalculateEstimatedTimesForAllVisitsAsync(int agendaId, IEnumerable<int> visitIds)
        {
            var result = new Dictionary<int, TimeOnly>();

            // Pobierz agendę z wizytami
            var agenda = await _uow.Agenda.GetAsync(
                filter: a => a.Id == agendaId,
                includeProperties: "Visits,Day"
            );

            if (agenda == null)
                return result;

            // Pobierz jednostkę czasową
            var minutesPerVisit = await GetMinutesPerVisitForAgendaAsync(agendaId);

            // Oblicz godzinę rozpoczęcia i zakończenia agendy
            var startHour = agenda.StartHourOverride ?? agenda.Day.StartHour;
            var endHour = agenda.EndHourOverride ?? agenda.Day.EndHour;

            // Posortuj wizyty według numeru porządkowego
            var sortedVisits = agenda.Visits
                .Where(v => visitIds.Contains(v.Id))
                .OrderBy(v => v.OrdinalNumber)
                .ToList();

            // Oblicz czasy dla wszystkich wizyt
            for (int i = 0; i < sortedVisits.Count; i++)
            {
                var visit = sortedVisits[i];
                
                // Oblicz przewidywaną godzinę: start + (pozycja * jednostka czasowa)
                var estimatedMinutes = i * minutesPerVisit;
                var estimatedTime = startHour.AddMinutes(estimatedMinutes);

                // Upewnij się, że nie przekraczamy godziny zakończenia
                if (estimatedTime > endHour)
                    estimatedTime = endHour;

                result[visit.Id] = estimatedTime;
            }

            return result;
        }

        /// <inheritdoc />
        public async Task<(TimeOnly Start, TimeOnly End)?> CalculateDynamicTimeRangeAsync(int visitId)
        {
            // Pobierz wizytę z pełnymi danymi (tracked query aby uniknąć cyklu)
            var visit = await _uow.Visit.GetAsync(
                filter: v => v.Id == visitId,
                includeProperties: "Agenda.Day,Agenda.Visits",
                tracked: true
            );

            if (visit == null || visit.Agenda == null)
                return null;

            // Jeśli wizyta ma inny status niż zaplanowana lub wstrzymana, zwróć null
            if (visit.Status != VisitStatus.Planned && visit.Status != VisitStatus.Suspended)
                return null;

            // Sprawdź, czy wizyty są ukryte
            if (visit.Agenda.HideVisits)
                return null;

            var agenda = visit.Agenda;

            // Oblicz datę i godzinę rozpoczęcia agendy
            var agendaStartDateTime = new DateTime(agenda.Day.Date, agenda.StartHourOverride ?? agenda.Day.StartHour);
            
            // Użyj czasu polskiego (UTC+1/UTC+2 zależnie od DST)
            var polandTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Central European Standard Time");
            var now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, polandTimeZone);

            // Sprawdź, czy agenda już się rozpoczęła (są wizyty ze statusem Visited lub Pending)
            var hasStarted = now >= agendaStartDateTime.AddMinutes(-15) && 
                agenda.Visits.Any(v => 
                    v.Status == VisitStatus.Visited || 
                    v.Status == VisitStatus.Rejected || 
                    v.Status == VisitStatus.Suspended || 
                    v.Status == VisitStatus.Pending);

            int positionInQueue;
            TimeOnly baseTime;

            if (hasStarted)
            {
                // Kolęda rozpoczęta - liczymy rzeczywistą pozycję
                var orderedVisits = agenda.Visits
                    .Where(v => v.Status != VisitStatus.Visited && 
                                v.Status != VisitStatus.Pending &&
                                v.Status != VisitStatus.Rejected &&
                                v.Status != VisitStatus.Suspended)
                    .OrderBy(v => v.OrdinalNumber)
                    .ToList();

                var currentVisitIndex = orderedVisits.FindIndex(v => v.Id == visitId);

                if (currentVisitIndex < 0)
                    return null;

                // Oblicz pozycję w kolejce (od ostatniej odbytej wizyty)
                positionInQueue = currentVisitIndex + 1;
                    
                // Oblicz bazowy czas jako max(teraz, godzina rozpoczęcia agendy) z uwzględnieniem daty
                baseTime = TimeOnly.FromDateTime(agendaStartDateTime > now ? agendaStartDateTime : now);
        }
            else
            {
                // Kolęda nierozpoczęta - używamy statycznego algorytmu
                var orderedVisits = agenda.Visits
                    .OrderBy(v => v.OrdinalNumber)
                    .ToList();

                var currentVisitIndex = orderedVisits.FindIndex(v => v.Id == visitId);

                if (currentVisitIndex < 0)
                    return null;

                positionInQueue = currentVisitIndex + 1;
                baseTime = agenda.StartHourOverride ?? agenda.Day.StartHour;
            }

            // Pobierz jednostkę czasową
            var minutesPerVisit = await GetMinutesPerVisitForAgendaAsync(agenda.Id);

            // Oblicz środek przedziału
            var centerTime = baseTime.AddMinutes((positionInQueue - 1) * minutesPerVisit);

            // Oblicz szerokość przedziału
            var rangeWidthMinutes = CalculateRangeWidth(positionInQueue);
            var halfRange = rangeWidthMinutes / 2;

            // Oblicz przedział
            var startTime = centerTime.AddMinutes(-halfRange);
            var endTime = centerTime.AddMinutes(halfRange);

            // Przytnij do rzeczywistych godzin agendy
            var agendaStart = agenda.StartHourOverride ?? agenda.Day.StartHour;
            var agendaEnd = agenda.EndHourOverride ?? agenda.Day.EndHour;

            _logger.LogDebug("Calculated range for visit {VisitId}: {StartTime} - {EndTime} (Agenda: {AgendaStart} - {AgendaEnd})", 
                visitId, startTime, endTime, agendaStart, agendaEnd);

            // Nie zaczynamy przedziału wcześniej niż kwadrans przed rozpoczęciem
            var earliestStart = agendaStart.AddMinutes(-15);
            var latestStart = agendaStart.AddMinutes(15);
            if (startTime < agendaStart)
                startTime = earliestStart;
            
            if (endTime < latestStart)
                endTime = latestStart;

            // Nie kończymy później niż koniec agendy
            if (endTime > agendaEnd)
                endTime = agendaEnd;

            // Upewnij się, że start jest przed końcem
            if (startTime >= endTime)
            {
                // Jeśli przedział jest niepoprawny, zwróć mały przedział wokół końca
                startTime = agendaEnd.AddMinutes(-MIN_RANGE_MINUTES);
                endTime = agendaEnd;
            }

            // Zaokroglij do najbliższej 5-minutówki
            startTime = LanguageExtensions.RoundTo(startTime, 5, roundDown: true);
            endTime = LanguageExtensions.RoundTo(endTime, 5);

            return (startTime, endTime);
        }

        /// <inheritdoc />
        public int CalculateRangeWidth(int positionInQueue)
        {
            // Dla pozycji >= 10: 2h (120 min)
            if (positionInQueue >= 10)
                return MAX_RANGE_MINUTES;

            // Nieliniowe spadanie od 10. pozycji do 1. pozycji
            // Używamy funkcji kwadratowej dla płynnego przejścia
            // pozycja 10: 120 min
            // pozycja 5: ~52 min
            // pozycja 1: 15 min

            // Normalizuj pozycję do zakresu 0-1 (pozycja 1 = 1.0, pozycja 10 = 0.0)
            var normalizedPosition = (10.0 - positionInQueue) / 9.0;

            // Użyj funkcji kwadratowej: y = ax^2 + b
            // Dla x=0 (pozycja 10): y=120
            // Dla x=1 (pozycja 1): y=15
            var range = MIN_RANGE_MINUTES + (MAX_RANGE_MINUTES - MIN_RANGE_MINUTES) * Math.Pow(1 - normalizedPosition, 2);

            return (int)Math.Round(range);
        }
    }
}
