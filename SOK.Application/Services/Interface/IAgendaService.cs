using SOK.Application.Common.DTO;
using SOK.Domain.Entities.Parish;

namespace SOK.Application.Services.Interface
{
    /// <summary>
    /// Serwis do zarządzania agendami.
    /// </summary>
    public interface IAgendaService
    {
        /// <summary>
        /// Pobiera wszystkie agendy dla danego dnia.
        /// </summary>
        /// <param name="dayId">Identyfikator dnia.</param>
        /// <returns>Lista agend.</returns>
        Task<List<AgendaDto>> GetAgendasForDayAsync(int dayId);

        /// <summary>
        /// Pobiera szczegóły agendy.
        /// </summary>
        /// <param name="agendaId">Identyfikator agendy.</param>
        /// <returns>Szczegóły agendy lub null jeśli nie znaleziono.</returns>
        Task<AgendaDto?> GetAgendaAsync(int agendaId);

        /// <summary>
        /// Tworzy nową agendę.
        /// </summary>
        /// <param name="dto">Dane agendy.</param>
        /// <returns>Identyfikator utworzonej agendy.</returns>
        Task<int> CreateAgendaAsync(SaveAgendaDto dto);

        /// <summary>
        /// Aktualizuje istniejącą agendę.
        /// </summary>
        /// <param name="dto">Zaktualizowane dane agendy.</param>
        Task UpdateAgendaAsync(SaveAgendaDto dto);

        /// <summary>
        /// Usuwa agendę.
        /// </summary>
        /// <param name="agendaId">Identyfikator agendy do usunięcia.</param>
        Task DeleteAgendaAsync(int agendaId);

        /// <summary>
        /// Pobiera dostępnych księży dla danego dnia (księży przypisanych do planu).
        /// </summary>
        /// <param name="dayId">Identyfikator dnia.</param>
        /// <returns>Lista dostępnych księży.</returns>
        Task<List<ParishMemberSimpleDto>> GetAvailablePriestsForDayAsync(int dayId);

        /// <summary>
        /// Pobiera dostępnych ministrantów.
        /// </summary>
        /// <returns>Lista wszystkich ministrantów.</returns>
        Task<List<ParishMemberSimpleDto>> GetAvailableMinistersAsync();

        // === Metody edytora agendy ===

        /// <summary>
        /// Pobiera wizyty dla agendy posortowane według kolejności.
        /// </summary>
        /// <param name="agendaId">Identyfikator agendy.</param>
        /// <returns>Lista wizyt w agendzie.</returns>
        Task<List<AgendaVisitDto>> GetAgendaVisitsAsync(int agendaId);

        /// <summary>
        /// Pobiera dostępne bramy z ich zgłoszeniami dla danego dnia.
        /// </summary>
        /// <param name="dayId">Identyfikator dnia.</param>
        /// <param name="streetId">Opcjonalny filtr ulicy.</param>
        /// <param name="scheduleId">Opcjonalny filtr harmonogramu.</param>
        /// <param name="onlyUnassigned">Czy pokazywać tylko bramy z nieprzypisanymi zgłoszeniami.</param>
        /// <returns>Lista bram z zgłoszeniami.</returns>
        Task<List<BuildingWithSubmissionsDto>> GetAvailableBuildingsAsync(
            int dayId, 
            int? streetId = null, 
            int? scheduleId = null, 
            bool onlyUnassigned = false);

        /// <summary>
        /// Przypisuje zgłoszenia do agendy.
        /// </summary>
        /// <param name="dto">Dane przypisania.</param>
        Task AssignVisitsToAgendaAsync(AssignVisitsToAgendaDto dto);

        /// <summary>
        /// Aktualizuje kolejność wizyt w agendzie.
        /// </summary>
        /// <param name="dto">Nowa kolejność wizyt.</param>
        Task UpdateVisitsOrderAsync(UpdateVisitsOrderDto dto);

        /// <summary>
        /// Usuwa wizyty z agendy (ustawia status na Unplanned).
        /// </summary>
        /// <param name="dto">Wizyty do usunięcia.</param>
        Task RemoveVisitsFromAgendaAsync(RemoveVisitsFromAgendaDto dto);

        /// <summary>
        /// Przenosi wizyty z jednej agendy do drugiej.
        /// </summary>
        /// <param name="dto">Dane przeniesienia.</param>
        // Task ReassignVisitsToAgendaAsync(ReassignVisitsToAgendaDto dto);

        // === Metody przeprowadzania wizyty ===

        /// <summary>
        /// Pobiera agendę na podstawie UniqueId (do autoryzacji).
        /// </summary>
        /// <param name="uniqueId">UniqueId agendy.</param>
        /// <returns>Szczegóły agendy lub null jeśli nie znaleziono.</returns>
        Task<AgendaDto?> GetAgendaByUniqueIdAsync(Guid uniqueId);

        /// <summary>
        /// Przypisuje księża do agendy.
        /// </summary>
        /// <param name="agendaId">Identyfikator agendy.</param>
        /// <param name="priestId">Identyfikator księża.</param>
        Task AssignPriestToAgendaAsync(int agendaId, int priestId);

        /// <summary>
        /// Aktualizuje kwotę zebranych funduszy w agendzie.
        /// </summary>
        /// <param name="agendaId">Identyfikator agendy.</param>
        /// <param name="gatheredFunds">Kwota zebranych funduszy.</param>
        Task UpdateGatheredFundsAsync(int agendaId, float gatheredFunds);

        /// <summary>
        /// Wstawia nową wizytę do agendy w odpowiedniej pozycji zgodnie z zasadami sortowania.
        /// </summary>
        /// <param name="agendaId">Identyfikator agendy.</param>
        /// <param name="submissionId">Identyfikator zgłoszenia.</param>
        Task InsertVisitToAgendaAsync(int agendaId, int submissionId);

        // === Metody metadanych ===

        /// <summary>
        /// Pobiera wartość liczbową metadanych dla agendy.
        /// </summary>
        /// <param name="agenda">Agenda.</param>
        /// <param name="metadataKey">Klucz metadanych.</param>
        /// <returns>Wartość liczbowa lub null.</returns>
        Task<int?> GetIntMetadataAsync(Agenda agenda, string metadataKey);

        /// <summary>
        /// Ustawia wartość liczbową metadanych dla agendy.
        /// </summary>
        /// <param name="agenda">Agenda.</param>
        /// <param name="metadataKey">Klucz metadanych.</param>
        /// <param name="value">Wartość do zapisania.</param>
        Task SetIntMetadataAsync(Agenda agenda, string metadataKey, int value);

        /// <summary>
        /// Usuwa metadane dla agendy.
        /// </summary>
        /// <param name="agenda">Agenda.</param>
        /// <param name="metadataKey">Klucz metadanych.</param>
        Task DeleteMetadataAsync(Agenda agenda, string metadataKey);
    }
}
