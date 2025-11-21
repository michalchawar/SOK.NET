using SOK.Domain.Entities.Parish;
using System.Linq.Expressions;

namespace SOK.Application.Services.Interface
{
    /// <summary>
    /// Usługa do obsługi obiektów <see cref="Street"/>.
    /// </summary>
    public interface IStreetService
    {
        /// <summary>
        /// Pobiera ulicę o podanym identifykatorze.
        /// </summary>
        /// <param name="id">Identyfikator ulicy do pobrania.</param>
        /// <returns>
        /// Obiekt <see cref="Task"/>, reprezentujący operację asynchroniczną,
        /// którego zawartością jest obiekt <see cref="Street"/> lub <see cref="null"/>,
        /// </returns>
        /// <remarks>
        /// Jeśli ulica o podanym identyfikatorze nie istnieje, zwracane jest <see cref="null"/>.
        /// </remarks>
        Task<Street?> GetStreetAsync(int id);

        /// <summary>
        /// Pobiera listę ulic, spełniających podany filtr, w porządku alfabetycznym.
        /// </summary>
        /// <param name="filter">Filtr, który spełniać mają ulice.</param>
        /// <param name="buildings">Określa, czy należy załadować powiązane budynki.</param>
        /// <param name="type">Określa, czy należy załadować powiązany rodzaj ulicy.</param>
        /// <returns>
        /// Obiekt <see cref="Task"/>, reprezentujący operację asynchroniczną,
        /// którego zawartością jest lista obiektów <see cref="Street"/>.
        /// </returns>
        /// <remarks>
        /// Jeśli nie jest ustawiony żaden filtr, funkcja zwraca wszystkie ulice.
        /// Jeśli zaś nie ma żadnej ulicy lub filtr nie pasuje do żadnej ulicy, zwracana jest pusta lista.
        /// </remarks>
        Task<IEnumerable<Street>> GetAllStreetsAsync(
            Expression<Func<Street, bool>>? filter = null,
            bool buildings = false,
            bool type = false);

        /// <summary>
        /// Zapisuje ulicę w bazie danych.
        /// </summary>
        /// <param name="street">Ulica, która ma zostać zapisana.</param>
        /// <returns>
        /// Obiekt <see cref="Task"/>, reprezentujący operację asynchroniczną.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// Jeśli ulica o takich danych już istnieje.
        /// </exception>
        Task CreateStreetAsync(Street street);

        /// <summary>
        /// Usuwa ulicę o podanym identyfikatorze.
        /// </summary>
        /// <param name="id">Id ulicy, która ma zostać usunięta.</param>
        /// <returns>
        /// Obiekt <see cref="Task"/>, reprezentujący operację asynchroniczną,
        /// którego zawartością jest wartość logiczna określająca, czy usunięcie się powiodło.
        /// </returns>
        Task<bool> DeleteStreetAsync(int id);

        /// <summary>
        /// Aktualizuje ulicę w bazie danych.
        /// </summary>
        /// <param name="street">Ulica, która ma zostać zaktualizowana.</param>
        /// <returns>
        /// Obiekt <see cref="Task"/>, reprezentujący operację asynchroniczną.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// Jeśli ulica o takich danych już istnieje.
        /// </exception>
        Task UpdateStreetAsync(Street street);

        /// <summary>
        /// Pobiera listę typów ulic, spełniających podany filtr, w porządku alfabetycznym.
        /// </summary>
        /// <param name="filter">Filtr, który spełniać mają typy.</param>
        /// <returns>
        /// Obiekt <see cref="Task"/>, reprezentujący operację asynchroniczną,
        /// którego zawartością jest lista obiektów <see cref="StreetSpecifier"/>.
        /// </returns>
        /// <remarks>
        /// Jeśli nie jest ustawiony żaden filtr, funkcja zwraca wszystkie typy.
        /// Jeśli zaś nie ma żadnego typu lub filtr nie pasuje do żadnego typu, zwracana jest pusta lista.
        /// </remarks>
        Task<IEnumerable<StreetSpecifier>> GetAllStreetSpecifiersAsync(
            Expression<Func<StreetSpecifier, bool>>? filter = null);
    }
}