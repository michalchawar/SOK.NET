using SOK.Application.Common.DTO.Address;
using SOK.Domain.Entities.Parish;

namespace SOK.Application.Services.Interface
{
    /// <summary>
    /// Usługa do obsługi obiektów <see cref="Building"/>.
    /// </summary>
    public interface IBuildingService
    {
        /// <summary>
        /// Pobiera budynek o podanym identifykatorze.
        /// </summary>
        /// <param name="id">Identyfikator budynku do pobrania.</param>
        /// <returns>
        /// Obiekt <see cref="Task"/>, reprezentujący operację asynchroniczną,
        /// którego zawartością jest obiekt <see cref="Building"/> lub <see cref="null"/>,
        /// </returns>
        /// <remarks>
        /// Jeśli budynek o podanym identyfikatorze nie istnieje, zwracane jest <see cref="null"/>.
        /// </remarks>
        Task<Building?> GetBuildingAsync(int id);

        /// <summary>
        /// Pobiera stronę budynków spełniających podany filtr.
        /// </summary>
        /// <param name="filter">Filtr, który spełniać mają budynki.</param>
        /// <param name="page">Numer strony.</param>
        /// <param name="pageSize">Liczba obiektów na stronie.</param>
        /// <returns>
        /// Obiekt <see cref="Task"/>, reprezentujący operację asynchroniczną,
        /// którego zawartością jest lista obiektów <see cref="Building"/>.
        /// </returns>
        /// <remarks>
        /// Jeśli nie jest ustawiony żaden filtr, funkcja zwraca wszystkie budynki podzielone na strony.
        /// Jeśli zaś nie ma żadnego budynku lub filtr nie pasuje do żadnego budynku, zwracana jest pusta lista.
        /// </remarks>
        /// <exception cref="ArgumentException">
        /// Jeśli podany numer strony lub rozmiar strony jest mniejszy niż 1.
        /// </exception>
        //Task<IEnumerable<Building>> GetBuildingsPaginatedAsync(
        //    Expression<Func<Building, bool>>? filter = null,
        //    int page = 1,
        //    int pageSize = 1);

        /// <summary>
        /// Zapisuje budynek w bazie danych.
        /// </summary>
        /// <param name="building">Budynek, który ma zostać zapisany.</param>
        /// <returns>
        /// Obiekt <see cref="Task"/>, reprezentujący operację asynchroniczną.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// Jeśli budynek o takich danych istnieje już na danej ulicy.
        /// </exception>
        Task CreateBuildingAsync(Building building);

        /// <summary>
        /// Usuwa budynek o podanym identyfikatorze.
        /// </summary>
        /// <param name="id">Id budynku, który ma zostać usunięty.</param>
        /// <returns>
        /// Obiekt <see cref="Task"/>, reprezentujący operację asynchroniczną,
        /// którego zawartością jest wartość logiczna określająca, czy usunięcie się powiodło.
        /// </returns>
        Task<bool> DeleteBuildingAsync(int id);

        /// <summary>
        /// Aktualizuje budynek w bazie danych.
        /// </summary>
        /// <param name="building">Budynek, który ma zostać zaktualizowany.</param>
        /// <returns>
        /// Obiekt <see cref="Task"/>, reprezentujący operację asynchroniczną.
        /// </returns>
        /// <exception cref="InalidOperationException">
        /// Jeśli budynek o takich danych istnieje już na danej ulicy.
        /// </exception>
        Task UpdateBuildingAsync(Building building);

        /// <summary>
        /// Pobiera wszystkie budynki wraz z danymi ulicy i miasta.
        /// </summary>
        /// <returns>Lista uproszczonych DTO budynków.</returns>
        Task<List<BuildingSimpleDto>> GetAllBuildingsAsync();
    }
}