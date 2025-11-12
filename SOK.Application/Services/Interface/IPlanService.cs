using SOK.Application.Common.DTO;
using SOK.Domain.Entities.Parish;
using System.Linq.Expressions;

namespace SOK.Application.Services.Interface
{
    /// <summary>
    /// Usługa do obsługi obiektów <see cref="Plan"/>.
    /// </summary>
    public interface IPlanService
    {
        /// <summary>
        /// Pobiera plan o podanym identyfikatorze.
        /// </summary>
        /// <param name="id">Identyfikator planu do pobrania.</param>
        /// <returns>
        /// Obiekt <see cref="Task"/>, reprezentujący operację asynchroniczną,
        /// którego zawartością jest obiekt <see cref="Plan"/> lub <see cref="null"/>,
        /// </returns>
        /// <remarks>
        /// Jeśli plan o podanym identyfikatorze nie istnieje, zwracane jest <see cref="null"/>.
        /// </remarks>
        Task<Plan?> GetPlanAsync(int id);

        /// <summary>
        /// Pobiera stronę planów spełniających podany filtr.
        /// </summary>
        /// <param name="filter">Filtr, który spełniać mają plany.</param>
        /// <param name="page">Numer strony.</param>
        /// <param name="pageSize">Liczba obiektów na stronie.</param>
        /// <returns>
        /// Obiekt <see cref="Task"/>, reprezentujący operację asynchroniczną,
        /// którego zawartością jest lista obiektów <see cref="Plan"/>.
        /// </returns>
        /// <remarks>
        /// Jeśli nie jest ustawiony żaden filtr, funkcja zwraca wszystkie plany podzielone na strony.
        /// Jeśli zaś nie ma żadnego planu lub filtr nie pasuje do żadnego planu, zwracana jest pusta lista.
        /// </remarks>
        /// <exception cref="ArgumentException">
        /// Jeśli podany numer strony lub rozmiar strony jest mniejszy niż 1.
        /// </exception>
        Task<List<Plan>> GetPlansPaginatedAsync(
            Expression<Func<Plan, bool>>? filter = null,
            int page = 1,
            int pageSize = 1);

        /// <summary>
        /// Zapisuje plan w bazie danych.
        /// </summary>
        /// <param name="plan">Obiekt <see cref="PlanActionRequestDto"/> z danymi planu, który ma zostać utworzony.</param>
        /// <returns>
        /// Obiekt <see cref="Task"/>, reprezentujący operację asynchroniczną.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// Jeśli ponad jeden obiekt <see cref="PlanScheduleDto"/> jest oznaczony flagą IsDefault.
        /// </exception>
        Task CreatePlanAsync(PlanActionRequestDto plan);
        
        /// <summary>
        /// Aktualizuje plan w bazie danych, wraz z jego powiązanymi danymi.
        /// </summary>
        /// <param name="plan">Obiekt <see cref="PlanActionRequestDto"/> z danymi planu, który ma zostać zaktualizowany.</param>
        /// <returns>
        /// Obiekt <see cref="Task"/>, reprezentujący operację asynchroniczną.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// Jeśli ponad jeden obiekt <see cref="PlanScheduleDto"/> jest oznaczony flagą IsDefault.
        /// </exception>
        Task UpdatePlanAsync(PlanActionRequestDto plan);

        /// <summary>
        /// Usuwa plan o podanym identyfikatorze.
        /// </summary>
        /// <param name="id">Id planu, który ma zostać usunięty.</param>
        /// <returns>
        /// Obiekt <see cref="Task"/>, reprezentujący operację asynchroniczną,
        /// którego zawartością jest wartość logiczna określająca, czy usunięcie się powiodło.
        /// </returns>
        Task<bool> DeletePlanAsync(int id);

        /// <summary>
        /// Aktualizuje plan w bazie danych.
        /// </summary>
        /// <param name="plan">Plan, który ma zostać zaktualizowany.</param>
        /// <returns>
        /// Obiekt <see cref="Task"/>, reprezentujący operację asynchroniczną.
        /// </returns>
        Task UpdatePlanAsync(Plan plan);

        /// <summary>
        /// Ustawia aktywny plan dla systemu.
        /// </summary>
        /// <param name="plan">Plan, który ma być aktywny.</param>
        /// <returns>
        /// Obiekt <see cref="Task"/>, reprezentujący operację asynchroniczną.
        /// </returns>
        Task SetActivePlanAsync(Plan plan);

        /// <summary>
        /// Pobiera aktywny plan dla systemu (jeśli istnieje).
        /// </summary>
        /// <returns>
        /// Obiekt <see cref="Task"/>, reprezentujący operację asynchroniczną,
        /// którego zawartością jest obiekt <see cref="Plan"/> lub <see cref="null"/>.
        /// </returns>
        Task<Plan?> GetActivePlanAsync();

        /// <summary>
        /// Usuwa informację o aktywnym planie dla systemu.
        /// </summary>
        /// <returns>
        /// Obiekt <see cref="Task"/>, reprezentujący operację asynchroniczną.
        /// </returns>
        Task ClearActivePlanAsync();
    }
}