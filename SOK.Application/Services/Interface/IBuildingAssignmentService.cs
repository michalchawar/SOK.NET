using SOK.Application.Common.DTO;
using SOK.Domain.Entities.Parish;

namespace SOK.Application.Services.Interface
{
    /// <summary>
    /// Serwis do zarządzania przypisaniami budynków do dni kolędowych.
    /// </summary>
    public interface IBuildingAssignmentService
    {
        /// <summary>
        /// Pobiera wszystkie budynki z informacjami o przypisaniach dla danego dnia.
        /// </summary>
        /// <param name="dayId">Identyfikator dnia.</param>
        /// <returns>Lista budynków z informacjami o przypisaniach.</returns>
        Task<List<BuildingWithAssignmentInfoDto>> GetBuildingsForDayAsync(int dayId);

        /// <summary>
        /// Przypisuje budynki do dnia w ramach harmonogramu.
        /// </summary>
        /// <param name="dayId">Identyfikator dnia.</param>
        /// <param name="scheduleId">Identyfikator harmonogramu.</param>
        /// <param name="buildingIds">Lista identyfikatorów budynków do przypisania.</param>
        /// <param name="agendaId">Opcjonalny identyfikator agendy do której mają być przypisane nieprzypisane zgłoszenia.</param>
        Task AssignBuildingsToDayAsync(int dayId, int scheduleId, List<int> buildingIds, int? agendaId = null);

        /// <summary>
        /// Przypisuje budynki zakresem według podanych parametrów.
        /// </summary>
        /// <param name="parameters">Parametry zakresu.</param>
        /// <returns>Lista identyfikatorów przypisanych budynków.</returns>
        Task<List<int>> AssignBuildingsByRangeAsync(AssignRangeDto parameters);

        /// <summary>
        /// Usuwa przypisanie budynku od dnia.
        /// </summary>
        /// <param name="dayId">Identyfikator dnia.</param>
        /// <param name="buildingId">Identyfikator budynku.</param>
        /// <param name="scheduleId">Identyfikator harmonogramu.</param>
        Task UnassignBuildingFromDayAsync(int dayId, int buildingId, int scheduleId);

        /// <summary>
        /// Pobiera podsumowanie ulic dla danego dnia.
        /// </summary>
        /// <param name="dayId">Identyfikator dnia.</param>
        /// <returns>Lista podsumowań ulic.</returns>
        Task<List<StreetSummaryDto>> GetStreetsSummaryForDayAsync(int dayId);
    }
}
