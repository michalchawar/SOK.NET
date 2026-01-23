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

        /// <summary>
        /// Włącza lub wyłącza zbieranie zgłoszeń dla podanego planu.
        /// </summary>
        /// <param name="plan">Plan, dla którego włączane lub wyłączane jest zbieranie zgłoszeń.</param>
        /// <param name="isEnabled">Wartość określająca, czy zbieranie zgłoszeń ma być włączone (true) czy wyłączone (false).</param>
        /// <returns>
        /// Obiekt <see cref="Task"/>, reprezentujący operację asynchroniczną.
        /// </returns>
        Task ToggleSubmissionGatheringAsync(Plan plan, bool isEnabled);
        
        /// <summary>
        /// Sprawdza, czy zbieranie zgłoszeń jest włączone dla podanego planu.
        /// </summary>
        /// <param name="plan">Plan, dla którego sprawdzane jest zbieranie zgłoszeń.</param>
        /// <returns>
        /// Obiekt <see cref="Task"/>, reprezentujący operację asynchroniczną,
        /// którego zawartością jest wartość logiczna określająca, czy zbieranie zgłoszeń jest włączone.
        /// </returns>
        Task<bool> IsSubmissionGatheringEnabledAsync(Plan plan);

        /// <summary>
        /// Pobiera wartość metadanej typu <see cref="DateTime"/> dla planu.
        /// </summary>
        /// <param name="plan">Plan, dla którego pobierana jest metadana.</param>
        /// <param name="metadataKey">Klucz metadanej (użyj stałych z <see cref="Common.Helpers.PlanMetadataKeys"/>).</param>
        /// <returns>
        /// Obiekt <see cref="Task"/>, reprezentujący operację asynchroniczną,
        /// którego zawartością jest wartość <see cref="DateTime"/> lub <see cref="null"/>.
        /// </returns>
        Task<DateTime?> GetDateTimeMetadataAsync(Plan plan, string metadataKey);

        /// <summary>
        /// Ustawia wartość metadanej typu <see cref="DateTime"/> dla planu.
        /// </summary>
        /// <param name="plan">Plan, dla którego ustawiana jest metadana.</param>
        /// <param name="metadataKey">Klucz metadanej (użyj stałych z <see cref="Common.Helpers.PlanMetadataKeys"/>).</param>
        /// <param name="value">Wartość do zapisania.</param>
        /// <returns>
        /// Obiekt <see cref="Task"/>, reprezentujący operację asynchroniczną.
        /// </returns>
        Task SetDateTimeMetadataAsync(Plan plan, string metadataKey, DateTime value);

        /// <summary>
        /// Pobiera wartość metadanej typu <see cref="TimeOnly"/> dla planu.
        /// </summary>
        /// <param name="plan">Plan, dla którego pobierana jest metadana.</param>
        /// <param name="metadataKey">Klucz metadanej (użyj stałych z <see cref="Common.Helpers.PlanMetadataKeys"/>).</param>
        /// <returns>
        /// Obiekt <see cref="Task"/>, reprezentujący operację asynchroniczną,
        /// którego zawartością jest wartość <see cref="TimeOnly"/> lub <see cref="null"/>.
        /// </returns>
        Task<TimeOnly?> GetTimeMetadataAsync(Plan plan, string metadataKey);

        /// <summary>
        /// Ustawia wartość metadanej typu <see cref="TimeOnly"/> dla planu.
        /// </summary>
        /// <param name="plan">Plan, dla którego ustawiana jest metadana.</param>
        /// <param name="metadataKey">Klucz metadanej (użyj stałych z <see cref="Common.Helpers.PlanMetadataKeys"/>).</param>
        /// <param name="value">Wartość do zapisania.</param>
        /// <returns>
        /// Obiekt <see cref="Task"/>, reprezentujący operację asynchroniczną.
        /// </returns>
        Task SetTimeMetadataAsync(Plan plan, string metadataKey, TimeOnly value);

        /// <summary>
        /// Usuwa metadaną dla planu.
        /// </summary>
        /// <param name="plan">Plan, dla którego usuwana jest metadana.</param>
        /// <param name="metadataKey">Klucz metadanej do usunięcia.</param>
        /// <returns>
        /// Obiekt <see cref="Task"/>, reprezentujący operację asynchroniczną.
        /// </returns>
        Task DeleteMetadataAsync(Plan plan, string metadataKey);

        /// <summary>
        /// Pobiera wszystkie dni dla podanego planu.
        /// </summary>
        /// <param name="planId">Identyfikator planu.</param>
        /// <returns>
        /// Obiekt <see cref="Task"/>, reprezentujący operację asynchroniczną,
        /// którego zawartością jest lista obiektów <see cref="Day"/>.
        /// </returns>
        Task<List<Day>> GetDaysForPlanAsync(int planId);

        /// <summary>
        /// Pobiera pojedynczy dzień po identyfikatorze.
        /// </summary>
        /// <param name="dayId">Identyfikator dnia.</param>
        /// <returns>
        /// Obiekt <see cref="Task"/>, reprezentujący operację asynchroniczną,
        /// którego zawartością jest obiekt <see cref="Day"/> lub <see cref="null"/>.
        /// </returns>
        Task<Day?> GetDayAsync(int dayId);

        /// <summary>
        /// Zarządza dniami kolędowymi dla planu (bulk create/update/delete).
        /// Usuwa dni, które nie są w liście, aktualizuje istniejące i tworzy nowe.
        /// </summary>
        /// <param name="planId">Identyfikator planu.</param>
        /// <param name="days">Lista dni do zarządzania.</param>
        /// <param name="visitsStartDate">Data rozpoczęcia kolędy (zostanie zapisana w metadanych).</param>
        /// <param name="visitsEndDate">Data zakończenia kolędy (zostanie zapisana w metadanych).</param>
        /// <returns>
        /// Obiekt <see cref="Task"/>, reprezentujący operację asynchroniczną.
        /// </returns>
        Task ManageDaysAsync(int planId, List<Day> days, DateTime visitsStartDate, DateTime visitsEndDate);

        /// <summary>
        /// Pobiera statystyki kolędy dla planu.
        /// </summary>
        /// <param name="planId">Identyfikator planu.</param>
        /// <returns>
        /// Obiekt <see cref="Task"/>, reprezentujący operację asynchroniczną,
        /// którego zawartością jest obiekt <see cref="VisitStatsDto"/> lub <see cref="null"/>.
        /// </returns>
        Task<VisitStatsDto?> GetVisitStatsAsync(int planId);
    }
}