using SOK.Domain.Entities.Parish;

namespace SOK.Application.Services.Interface
{
    /// <summary>
    /// Serwis do obliczania przewidywanego czasu wizyty.
    /// </summary>
    public interface IVisitTimeEstimationService
    {
        /// <summary>
        /// Pobiera jednostkę czasową (minuty na wizytę) dla agendy.
        /// Jeśli agenda ma nadpisaną wartość, używa jej; w przeciwnym razie bierze z księdza lub domyślną.
        /// </summary>
        /// <param name="agendaId">Identyfikator agendy.</param>
        /// <returns>Liczba minut na wizytę.</returns>
        Task<int> GetMinutesPerVisitForAgendaAsync(int agendaId);

        /// <summary>
        /// Oblicza przewidywaną godzinę wizyty na podstawie pozycji w kolejce i jednostki czasowej.
        /// </summary>
        /// <param name="submissionId">Identyfikator zgłoszenia.</param>
        /// <returns>Przewidywana godzina wizyty lub null jeśli nie można obliczyć.</returns>
        Task<TimeOnly?> CalculateEstimatedTimeAsync(int submissionId);
        
        /// <summary>
        /// Oblicza przewidywaną godzinę wizyty na podstawie pozycji w kolejce i jednostki czasowej.
        /// </summary>
        /// <param name="agendaId">Identyfikator agendy.</param>
        /// <param name="visitId">Identyfikator wizyty.</param>
        /// <returns>Przewidywana godzina wizyty lub null jeśli nie można obliczyć.</returns>
        Task<TimeOnly?> CalculateEstimatedTimeAsync(int agendaId, int visitId);

        /// <summary>
        /// Oblicza przewidywane godziny dla wszystkich wizyt w agendzie na podstawie pozycji w kolejce.
        /// Używa statycznego algorytmu: startHour + (pozycja * minutesPerVisit).
        /// </summary>
        /// <param name="agendaId">Identyfikator agendy.</param>
        /// <param name="visits">Lista wizyt posortowana według OrdinalNumber.</param>
        /// <returns>Słownik mapujący VisitId na przewidywaną godzinę.</returns>
        Task<Dictionary<int, TimeOnly>> CalculateEstimatedTimesForAllVisitsAsync(int agendaId, IEnumerable<int> visitIds);

        /// <summary>
        /// Oblicza dynamiczny przedział czasowy wizyty dla zgłaszającego.
        /// Po rozpoczęciu kolędy liczy rzeczywistą pozycję w kolejce.
        /// </summary>
        /// <param name="visitId">Identyfikator wizyty.</param>
        /// <returns>Przedział czasowy (start, koniec) lub null jeśli wizyta jest ukryta.</returns>
        Task<(TimeOnly Start, TimeOnly End)?> CalculateDynamicTimeRangeAsync(int visitId);

        /// <summary>
        /// Oblicza szerokość przedziału czasowego na podstawie miejsca w kolejce.
        /// </summary>
        /// <param name="positionInQueue">Miejsce w kolejce.</param>
        /// <returns>Szerokość przedziału w minutach.</returns>
        int CalculateRangeWidth(int positionInQueue);
    }
}
