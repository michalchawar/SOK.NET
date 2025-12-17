using SOK.Domain.Entities.Parish;

namespace SOK.Application.Common.Interface
{
    /// <summary>
    /// Reprezentuje repozytorium przypisań budynków do dni.
    /// </summary>
    public interface IBuildingAssignmentRepository : IUpdatableRepository<BuildingAssignment>
    {
        /// <summary>
        /// Pobiera wszystkie przypisania dla danego dnia.
        /// </summary>
        /// <param name="dayId">Identyfikator dnia.</param>
        /// <returns>Lista przypisań.</returns>
        Task<List<BuildingAssignment>> GetAssignmentsForDayAsync(int dayId);

        /// <summary>
        /// Pobiera przypisanie budynku do dnia w ramach harmonogramu.
        /// </summary>
        /// <param name="dayId">Identyfikator dnia.</param>
        /// <param name="buildingId">Identyfikator budynku.</param>
        /// <param name="scheduleId">Identyfikator harmonogramu.</param>
        /// <returns>Przypisanie lub null.</returns>
        Task<BuildingAssignment?> GetAssignmentAsync(int dayId, int buildingId, int scheduleId);

        /// <summary>
        /// Sprawdza czy budynek jest przypisany do jakiegokolwiek dnia.
        /// </summary>
        /// <param name="buildingId">Identyfikator budynku.</param>
        /// <param name="scheduleId">Identyfikator harmonogramu.</param>
        /// <returns>True jeśli przypisany.</returns>
        Task<bool> IsBuildingAssignedAsync(int buildingId, int scheduleId);
    }
}
