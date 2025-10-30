using SOK.Domain.Entities.Parish;
using System.Linq.Expressions;

namespace SOK.Application.Services.Interface
{
    /// <summary>
    /// Us³uga do obs³ugi obiektów <see cref="Street"/>.
    /// </summary>
    public interface IStreetService
    {
        /// <summary>
        /// Pobiera ulicê o podanym identifykatorze.
        /// </summary>
        /// <param name="id">Identyfikator ulicy do pobrania.</param>
        /// <returns>
        /// Obiekt <see cref="Task"/>, reprezentuj¹cy operacjê asynchroniczn¹,
        /// którego zawartoœci¹ jest obiekt <see cref="Street"/> lub <see cref="null"/>,
        /// </returns>
        /// <remarks>
        /// Jeœli ulica o podanym identyfikatorze nie istnieje, zwracane jest <see cref="null"/>.
        /// </remarks>
        Task<Street?> GetStreetAsync(int id);

        /// <summary>
        /// Pobiera listê ulic, spe³niaj¹cych podany filtr, w porz¹dku alfabetycznym.
        /// </summary>
        /// <param name="filter">Filtr, który spe³niaæ maj¹ ulice.</param>
        /// <param name="buildings">Okreœla, czy nale¿y za³adowaæ powi¹zane budynki.</param>
        /// <returns>
        /// Obiekt <see cref="Task"/>, reprezentuj¹cy operacjê asynchroniczn¹,
        /// którego zawartoœci¹ jest lista obiektów <see cref="Street"/>.
        /// </returns>
        /// <remarks>
        /// Jeœli nie jest ustawiony ¿aden filtr, funkcja zwraca wszystkie ulice.
        /// Jeœli zaœ nie ma ¿adnej ulicy lub filtr nie pasuje do ¿adnej ulicy, zwracana jest pusta lista.
        /// </remarks>
        Task<IEnumerable<Street>> GetAllStreetsAsync(
            Expression<Func<Street, bool>>? filter = null,
            bool buildings = false);

        /// <summary>
        /// Zapisuje ulicê w bazie danych.
        /// </summary>
        /// <param name="street">Ulica, która ma zostaæ zapisana.</param>
        /// <returns>
        /// Obiekt <see cref="Task"/>, reprezentuj¹cy operacjê asynchroniczn¹.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// Jeœli ulica o takich danych ju¿ istnieje.
        /// </exception>
        Task CreateStreetAsync(Street street);

        /// <summary>
        /// Usuwa ulicê o podanym identyfikatorze.
        /// </summary>
        /// <param name="id">Id ulicy, która ma zostaæ usuniêta.</param>
        /// <returns>
        /// Obiekt <see cref="Task"/>, reprezentuj¹cy operacjê asynchroniczn¹,
        /// którego zawartoœci¹ jest wartoœæ logiczna okreœlaj¹ca, czy usuniêcie siê powiod³o.
        /// </returns>
        Task<bool> DeleteStreetAsync(int id);

        /// <summary>
        /// Aktualizuje ulicê w bazie danych.
        /// </summary>
        /// <param name="street">Ulica, która ma zostaæ zaktualizowana.</param>
        /// <returns>
        /// Obiekt <see cref="Task"/>, reprezentuj¹cy operacjê asynchroniczn¹.
        /// </returns>
        Task UpdateStreetAsync(Street street);
    }
}