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
    }
}
