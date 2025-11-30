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
        /// Wysyła email potwierdzający zgłoszenie.
        /// </summary>
        /// <param name="submissionId">ID zgłoszenia</param>
        /// <returns>
        /// Obiekt <see cref="Task"/>, reprezentujący operację asynchroniczną,
        /// którego wartością jest wartość logiczna określająca, czy email został pomyślnie wysłany.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// Jeśli zgłoszenie o podanym ID nie istnieje.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Jeśli zgłaszający nie ma przypisanego adresu email.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Jeśli bazowy URL aplikacji nie został skonfigurowany.
        /// </exception>
        Task<bool> SendConfirmationEmailAsync(int submissionId);
        

        /// <summary>
        /// Tworzy podgląd emaila potwierdzającego zgłoszenie.
        /// </summary>
        /// <param name="submissionId">ID zgłoszenia</param>
        /// <returns>
        /// Obiekt <see cref="Task"/>, reprezentujący operację asynchroniczną,
        /// którego wartością jest treść HTML maila.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// Jeśli zgłoszenie o podanym ID nie istnieje.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Jeśli zgłaszający nie ma przypisanego adresu email.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Jeśli bazowy URL aplikacji nie został skonfigurowany.
        /// </exception>
        Task<string> PreviewConfirmationEmailAsync(int submissionId);
        

        /// <summary>
        /// Wysyła email z zapytaniem o błąd we wprowadzaniu adresu mailowego.
        /// </summary>
        /// <param name="submissionId">ID zgłoszenia</param>
        /// <param name="to">Adres email nowego odbiorcy</param>
        /// <returns>
        /// Obiekt <see cref="Task"/>, reprezentujący operację asynchroniczną,
        /// którego wartością jest wartość logiczna określająca, czy email został pomyślnie wysłany.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// Jeśli zgłoszenie o podanym ID nie istnieje.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Jeśli zgłaszający nie ma przypisanego adresu email.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Jeśli bazowy URL aplikacji nie został skonfigurowany.
        /// </exception>
        Task<bool> SendInvalidEmailAsync(int submissionId, string to = "");

        /// <summary>
        /// Tworzy podgląd emaila z zapytaniem o błąd we wprowadzaniu adresu mailowego.
        /// </summary>
        /// <param name="submissionId">ID zgłoszenia</param>
        /// <param name="to">Adres email nowego odbiorcy</param>
        /// <returns>
        /// Obiekt <see cref="Task"/>, reprezentujący operację asynchroniczną,
        /// którego wartością jest treść HTML maila.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// Jeśli zgłoszenie o podanym ID nie istnieje.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Jeśli zgłaszający nie ma przypisanego adresu email.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Jeśli bazowy URL aplikacji nie został skonfigurowany.
        /// </exception>
        Task<string> PreviewInvalidEmailAsync(int submissionId, string to = "");
    }
}