using SOK.Domain.Entities.Central;

namespace SOK.Application.Services.Interface
{
    /// <summary>
    /// Serwis obsługujący operacje dla superadministratora systemu.
    /// </summary>
    public interface ISuperAdminService
    {
        /// <summary>
        /// Pobiera wszystkie parafie w systemie.
        /// </summary>
        /// <returns>Lista wszystkich parafii.</returns>
        Task<IEnumerable<ParishEntry>> GetAllParishesAsync();

        /// <summary>
        /// Pobiera parafię po jej identyfikatorze.
        /// </summary>
        /// <param name="id">Identyfikator parafii.</param>
        /// <returns>Parafia lub null, jeśli nie znaleziono.</returns>
        Task<ParishEntry?> GetParishByIdAsync(int id);

        /// <summary>
        /// Aktualizuje nazwę parafii.
        /// </summary>
        /// <param name="id">Identyfikator parafii do aktualizacji.</param>
        /// <param name="newName">Nowa nazwa parafii.</param>
        /// <returns>True, jeśli aktualizacja zakończyła się sukcesem.</returns>
        Task<bool> UpdateParishNameAsync(int id, string newName);
    }
}
