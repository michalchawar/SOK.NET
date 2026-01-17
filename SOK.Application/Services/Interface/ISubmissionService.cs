using SOK.Application.Common.DTO;
using SOK.Domain.Entities.Parish;
using System.Linq.Expressions;

namespace SOK.Application.Services.Interface
{
    /// <summary>
    /// Usługa do obsługi obiektów <see cref="Submission"/>.
    /// </summary>
    public interface ISubmissionService
    {
        /// <summary>
        /// Pobiera zgłoszenie o podanym identifykatorze.
        /// </summary>
        /// <param name="id">Identyfikator zgłoszenia do pobrania.</param>
        /// <returns>
        /// Obiekt <see cref="Task"/>, reprezentujący operację asynchroniczną,
        /// którego zawartością jest obiekt <see cref="Submission"/> lub <see cref="null"/>,
        /// </returns>
        /// <remarks>
        /// Jeśli zgłoszenie o podanym identyfikatorze nie istnieje, zwracane jest <see cref="null"/>.
        /// </remarks>
        Task<Submission?> GetSubmissionAsync(int id);

        /// <summary>
        /// Pobiera zgłoszenie o podanym unikalnym identifykatorze (UID).
        /// </summary>
        /// <param name="id">Unikalny identifykator (UID) zgłoszenia do pobrania.</param>
        /// <returns>
        /// Obiekt <see cref="Task"/>, reprezentujący operację asynchroniczną,
        /// którego zawartością jest obiekt <see cref="Submission"/> lub <see cref="null"/>,
        /// </returns>
        /// <remarks>
        /// Jeśli zgłoszenie o podanym UID nie istnieje, zwracane jest <see cref="null"/>.
        /// </remarks>
        Task<Submission?> GetSubmissionAsync(string uid);

        /// <summary>
        /// Pobiera stronę zgłoszeń spełniających podany filtr.
        /// </summary>
        /// <param name="filter">Filtr, który spełniać mają zgłoszenia.</param>
        /// <param name="page">Numer strony.</param>
        /// <param name="pageSize">Liczba obiektów na stronie.</param>
        /// <returns>
        /// Obiekt <see cref="Task"/>, reprezentujący operację asynchroniczną,
        /// którego zawartością jest lista obiektów <see cref="Submission"/>.
        /// </returns>
        /// <remarks>
        /// Jeśli nie jest ustawiony żaden filtr, funkcja zwraca wszystkie zgłoszenia podzielone na strony.
        /// Jeśli zaś nie ma żadnego zgłoszenia lub filtr nie pasuje do żadnego zgłoszenia, zwracana jest pusta lista.
        /// </remarks>
        /// <exception cref="ArgumentException">
        /// Jeśli podany numer strony lub rozmiar strony jest mniejszy niż 1.
        /// </exception>
        Task<IEnumerable<Submission>> GetSubmissionsPaginated(
            Expression<Func<Submission, bool>>? filter = null,
            int page = 1,
            int pageSize = 1);

        /// <summary>
        /// Pobiera stronę zgłoszeń z sortowaniem i zwraca również całkowitą liczbę wyników.
        /// </summary>
        /// <param name="filter">Filtr, który spełniać mają zgłoszenia.</param>
        /// <param name="sortBy">Pole sortowania: time, address, submitter</param>
        /// <param name="order">Kierunek sortowania: asc, desc</param>
        /// <param name="page">Numer strony.</param>
        /// <param name="pageSize">Liczba obiektów na stronie.</param>
        /// <returns>
        /// Obiekt <see cref="Task"/>, reprezentujący operację asynchroniczną,
        /// którego zawartością jest krotka z listą obiektów <see cref="Submission"/> i całkowitą liczbą wyników.
        /// </returns>
        Task<(IEnumerable<Submission> submissions, int totalCount)> GetSubmissionsPaginatedWithSorting(
            Expression<Func<Submission, bool>>? filter = null,
            string sortBy = "time",
            string order = "desc",
            int page = 1,
            int pageSize = 1);

        /// <summary>
        /// Zapisuje zgłoszenie w bazie danych.
        /// </summary>
        /// <param name="submissionDto">Obiekt <see cref="SubmissionCreationRequestDto"/> z danymi zgłoszenia, które ma zostać zapisane.</param>
        /// <returns>
        /// Obiekt <see cref="Task"/>, reprezentujący operację asynchroniczną,
        /// którego zawartością jest identyfikator nowo utworzonego zgłoszenia lub <see cref="null"/>,
        /// jeśli zgłoszenie nie zostało utworzone.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// Jeśli budynek lub harmonogram podane w zgłoszeniu nie istnieją w bazie danych.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Jeśli z podanym adresem powiązane jest już jakieś zgłoszenie.
        /// </exception>
        Task<int?> CreateSubmissionAsync(SubmissionCreationRequestDto submissionDto);

        /// <summary>
        /// Usuwa zgłoszenie o podanym identyfikatorze.
        /// </summary>
        /// <param name="id">Id zgłoszenia, które ma zostać usunięte.</param>
        /// <returns>
        /// Obiekt <see cref="Task"/>, reprezentujący operację asynchroniczną,
        /// którego zawartością jest wartość logiczna określająca, czy usunięcie się powiodło.
        /// </returns>
        Task<bool> DeleteSubmissionAsync(int id);

        /// <summary>
        /// Aktualizuje zgłoszenie w bazie danych.
        /// </summary>
        /// <param name="submission">Zgłoszenie, które ma zostać zaktualizowane.</param>
        /// <returns>
        /// Obiekt <see cref="Task"/>, reprezentujący operację asynchroniczną.
        /// </returns>
        Task UpdateSubmissionAsync(Submission submission);

        /// <summary>
        /// Wycofuje zgłoszenie (ustawia status wizyty na <see cref="VisitStatus.Withdrawn"/>).
        /// </summary>
        /// <param name="id">Identyfikator zgłoszenia.</param>
        /// <returns>
        /// Obiekt <see cref="Task"/>, reprezentujący operację asynchroniczną.
        /// </returns>
        Task WithdrawSubmissionAsync(int id);

        /// <summary>
        /// Przywraca zgłoszenie (ustawia status wizyty na <see cref="VisitStatus.Unplanned"/> w domyślnym harmonogramie).
        /// </summary>
        /// <param name="id">Identyfikator zgłoszenia.</param>
        /// <returns>
        /// Obiekt <see cref="Task"/>, reprezentujący operację asynchroniczną.
        /// </returns>
        Task RestoreSubmissionAsync(int id);

        /// <summary>
        /// Pobiera losowe zgłoszenie z bazy danych.
        /// </summary>
        /// <returns>
        /// Obiekt <see cref="Task"/>, reprezentujący operację asynchroniczną,
        /// którego wartością jest obiekt <see cref="Submission"/> lub <see cref="null"/>.
        /// </returns>
        /// <remarks>
        /// Jeśli w bazie nie ma żadnego zgłoszenia, zwracane jest <see cref="null"/>.
        /// </remarks>
        Task<Submission?> GetRandomSubmissionAsync();

        /// <summary>
        /// Znajduje zgłoszenie na podstawie adresu.
        /// </summary>
        /// <param name="buildingId">Identyfikator budynku.</param>
        /// <param name="apartmentNumber">Numer mieszkania.</param>
        /// <param name="apartmentLetter">Litera mieszkania.</param>
        /// <returns>Zgłoszenie lub null jeśli nie znaleziono.</returns>
        Task<Submission?> FindSubmissionByAddressAsync(int buildingId, int? apartmentNumber, string? apartmentLetter);

        /// <summary>
        /// Dodaje tekst do AdminNotes zgłoszenia.
        /// </summary>
        /// <param name="submissionId">Identyfikator zgłoszenia.</param>
        /// <param name="text">Tekst do dodania.</param>
        Task AppendAdminNotesAsync(int submissionId, string text);

        /// <summary>
        /// Tworzy nowe zgłoszenie podczas przeprowadzania wizyty (z generycznymi danymi).
        /// </summary>
        /// <param name="buildingId">Identyfikator budynku.</param>
        /// <param name="apartmentNumber">Numer mieszkania.</param>
        /// <param name="apartmentLetter">Litera mieszkania.</param>
        /// <param name="scheduleId">Identyfikator harmonogramu.</param>
        /// <returns>Identyfikator utworzonego zgłoszenia.</returns>
        Task<int> CreateSubmissionDuringVisitAsync(int buildingId, int? apartmentNumber, string? apartmentLetter, int scheduleId);
    }
}