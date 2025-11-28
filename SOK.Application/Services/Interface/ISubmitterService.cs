using SOK.Domain.Entities.Parish;
using System.Linq.Expressions;

namespace SOK.Application.Services.Interface
{
    /// <summary>
    /// Usługa do obsługi obiektów Submitter.
    /// </summary>
    public interface ISubmitterService
    {
        /// <summary>
        /// Pobiera zgłaszającego o podanym identifykatorze.
        /// </summary>
        /// <param name="id">Identyfikator zgłaszającego do pobrania.</param>
        /// <returns>
        /// Obiekt <see cref="Task"/>, reprezentujący operację asynchroniczną,
        /// którego zawartością jest obiekt <see cref="Submitter"/> lub <see cref="null"/>,
        /// </returns>
        /// <remarks>
        /// Jeśli zgłaszający o podanym identyfikatorze nie istnieje, zwracane jest <see cref="null"/>.
        /// </remarks>
        Task<Submitter?> GetSubmitterAsync(int id);

        /// <summary>
        /// Pobiera zgłaszającego o podanym unikalnym identyfikatorze (UID).
        /// </summary>
        /// <param name="uid">Unikalny identyfikator (UID) zgłaszającego do pobrania.</param>
        /// <returns>
        /// Obiekt <see cref="Task"/>, reprezentujący operację asynchroniczną,
        /// którego zawartością jest obiekt <see cref="Submitter"/> lub <see cref="null"/>,
        /// </returns>
        /// <remarks>
        /// Jeśli zgłaszający o podanym UID nie istnieje, zwracane jest <see cref="null"/>.
        /// </remarks>
        Task<Submitter?> GetSubmitterAsync(string uid);

        /// <summary>
        /// Pobiera stronę zgłaszających spełniających podany filtr.
        /// </summary>
        /// <param name="filter">Filtr, który spełniać mają zgłaszający.</param>
        /// <param name="page">Numer strony.</param>
        /// <param name="pageSize">Liczba obiektów na stronie.</param>
        /// <returns>
        /// Obiekt <see cref="Task"/>, reprezentujący operację asynchroniczną,
        /// którego zawartością jest lista obiektów <see cref="Submitter"/>.
        /// </returns>
        /// <remarks>
        /// Jeśli nie jest ustawiony żaden filtr, funkcja zwraca wszystkich zgłaszających podzielonych na strony.
        /// Jeśli zaś nie ma żadnych zgłaszających lub filtr nie pasuje do żadnego zgłaszającego, zwracana jest pusta lista.
        /// </remarks>
        /// <exception cref="ArgumentException">
        /// Jeśli podany numer strony lub rozmiar strony jest mniejszy niż 1.
        /// </exception>
        Task<IEnumerable<Submitter>> GetSubmittersPaginatedAsync(
            Expression<Func<Submitter, bool>>? filter = null,
            int page = 1,
            int pageSize = 1);

        /// <summary>
        /// Zapisuje zgłaszającego w bazie danych.
        /// </summary>
        /// <param name="submitter">Zgłaszający, który ma zostać zapisany.</param>
        /// <returns>
        /// Obiekt <see cref="Task"/>, reprezentujący operację asynchroniczną.
        /// </returns>
        Task CreateSubmitterAsync(Submitter submitter);

        /// <summary>
        /// Usuwa zgłaszającego o podanym identyfikatorze.
        /// </summary>
        /// <param name="id">Id zgłaszającego, który ma zostać usunięty.</param>
        /// <returns>
        /// Obiekt <see cref="Task"/>, reprezentujący operację asynchroniczną,
        /// którego zawartością jest wartość logiczna określająca, czy usunięcie się powiodło.
        /// </returns>
        Task<bool> DeleteSubmitterAsync(int id);

        /// <summary>
        /// Aktualizuje zgłaszającego w bazie danych.
        /// </summary>
        /// <param name="submitter">Zgłaszający, który ma zostać zaktualizowany.</param>
        /// <returns>
        /// Obiekt <see cref="Task"/>, reprezentujący operację asynchroniczną.
        /// </returns>
        Task UpdateSubmitterAsync(Submitter submitter);
    }
}
