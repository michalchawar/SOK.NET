using SOK.Domain.Entities.Parish;
using System.Linq.Expressions;

namespace SOK.Application.Services.Interface
{
    /// <summary>
    /// Us³uga do obs³ugi obiektów <see cref="Building"/>.
    /// </summary>
    public interface IBuildingService
    {
        /// <summary>
        /// Pobiera budynek o podanym identifykatorze.
        /// </summary>
        /// <param name="id">Identyfikator budynku do pobrania.</param>
        /// <returns>
        /// Obiekt <see cref="Task"/>, reprezentuj¹cy operacjê asynchroniczn¹,
        /// którego zawartoœci¹ jest obiekt <see cref="Building"/> lub <see cref="null"/>,
        /// </returns>
        /// <remarks>
        /// Jeœli budynek o podanym identyfikatorze nie istnieje, zwracane jest <see cref="null"/>.
        /// </remarks>
        Task<Building?> GetBuildingAsync(int id);

        /// <summary>
        /// Pobiera stronê budynków spe³niaj¹cych podany filtr.
        /// </summary>
        /// <param name="filter">Filtr, który spe³niaæ maj¹ budynki.</param>
        /// <param name="page">Numer strony.</param>
        /// <param name="pageSize">Liczba obiektów na stronie.</param>
        /// <returns>
        /// Obiekt <see cref="Task"/>, reprezentuj¹cy operacjê asynchroniczn¹,
        /// którego zawartoœci¹ jest lista obiektów <see cref="Building"/>.
        /// </returns>
        /// <remarks>
        /// Jeœli nie jest ustawiony ¿aden filtr, funkcja zwraca wszystkie budynki podzielone na strony.
        /// Jeœli zaœ nie ma ¿adnego budynku lub filtr nie pasuje do ¿adnego budynku, zwracana jest pusta lista.
        /// </remarks>
        /// <exception cref="ArgumentException">
        /// Jeœli podany numer strony lub rozmiar strony jest mniejszy ni¿ 1.
        /// </exception>
        //Task<IEnumerable<Building>> GetBuildingsPaginatedAsync(
        //    Expression<Func<Building, bool>>? filter = null,
        //    int page = 1,
        //    int pageSize = 1);

        /// <summary>
        /// Zapisuje budynek w bazie danych.
        /// </summary>
        /// <param name="building">Budynek, który ma zostaæ zapisany.</param>
        /// <returns>
        /// Obiekt <see cref="Task"/>, reprezentuj¹cy operacjê asynchroniczn¹.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// Jeœli budynek o takich danych istnieje ju¿ na danej ulicy.
        /// </exception>
        Task CreateBuildingAsync(Building building);

        /// <summary>
        /// Usuwa budynek o podanym identyfikatorze.
        /// </summary>
        /// <param name="id">Id budynku, który ma zostaæ usuniêty.</param>
        /// <returns>
        /// Obiekt <see cref="Task"/>, reprezentuj¹cy operacjê asynchroniczn¹,
        /// którego zawartoœci¹ jest wartoœæ logiczna okreœlaj¹ca, czy usuniêcie siê powiod³o.
        /// </returns>
        Task<bool> DeleteBuildingAsync(int id);

        /// <summary>
        /// Aktualizuje budynek w bazie danych.
        /// </summary>
        /// <param name="building">Budynek, który ma zostaæ zaktualizowany.</param>
        /// <returns>
        /// Obiekt <see cref="Task"/>, reprezentuj¹cy operacjê asynchroniczn¹.
        /// </returns>
        Task UpdateBuildingAsync(Building building);
    }
}