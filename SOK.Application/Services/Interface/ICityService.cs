using SOK.Domain.Entities.Parish;
using System.Linq.Expressions;

namespace SOK.Application.Services.Interface
{
    /// <summary>
    /// Usługa do obsługi obiektów <see cref="City"/>.
    /// </summary>
    public interface ICityService
    {
        /// <summary>
        /// Pobiera miasto o podanym identifykatorze.
        /// </summary>
        /// <param name="id">Identyfikator miasta do pobrania.</param>
        /// <returns>
        /// Obiekt <see cref="Task"/>, reprezentujący operację asynchroniczną,
        /// którego zawartością jest obiekt <see cref="City"/> lub <see cref="null"/>,
        /// </returns>
        /// <remarks>
        /// Jeśli miasto o podanym identyfikatorze nie istnieje, zwracane jest <see cref="null"/>.
        /// </remarks>
        Task<City?> GetCityAsync(int id);

        /// <summary>
        /// Pobiera listę miast, spełniających podany filtr, w porządku alfabetycznym.
        /// </summary>
        /// <param name="filter">Filtr, który spełniać mają miasta.</param>
        /// <param name="streets">Określa, czy należy załadować powiązane miaste.</param>
        /// <returns>
        /// Obiekt <see cref="Task"/>, reprezentujący operację asynchroniczną,
        /// którego zawartością jest lista obiektów <see cref="City"/>.
        /// </returns>
        /// <remarks>
        /// Jeśli nie jest ustawiony żaden filtr, funkcja zwraca wszystkie miasta.
        /// Jeśli zaś nie ma żadnego miasta lub filtr nie pasuje do żadnego miasta, zwracana jest pusta lista.
        /// </remarks>
        Task<IEnumerable<City>> GetAllCitiesAsync(
            Expression<Func<City, bool>>? filter = null,
            bool streets = false);

        /// <summary>
        /// Zapisuje miasto w bazie danych.
        /// </summary>
        /// <param name="City">Miasto, która ma zostać zapisane.</param>
        /// <returns>
        /// Obiekt <see cref="Task"/>, reprezentujący operację asynchroniczną.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// Jeśli miasto o takich danych już istnieje.
        /// </exception>
        Task CreateCityAsync(City City);

        /// <summary>
        /// Usuwa miasto o podanym identyfikatorze.
        /// </summary>
        /// <param name="id">Id miasta, która ma zostać usunięte.</param>
        /// <returns>
        /// Obiekt <see cref="Task"/>, reprezentujący operację asynchroniczną,
        /// którego zawartością jest wartość logiczna określająca, czy usunięcie się powiodło.
        /// </returns>
        Task<bool> DeleteCityAsync(int id);

        /// <summary>
        /// Aktualizuje miasto w bazie danych.
        /// </summary>
        /// <param name="City">miasto, która ma zostać zaktualizowane.</param>
        /// <returns>
        /// Obiekt <see cref="Task"/>, reprezentujący operację asynchroniczną.
        /// </returns>
        Task UpdateCityAsync(City City);
    }
}