using SOK.Domain.Entities.Parish;

namespace SOK.Application.Services.Interface
{
    /// <summary>
    /// Usługa do obsługi obiektów <see cref="Schedule"/>.
    /// </summary>
    public interface IScheduleService
    {
        /// <summary>
        /// Pobiera harmonogram o podanym identyfikatorze.
        /// </summary>
        /// <param name="id">Identyfikator harmonogramu do pobrania.</param>
        /// <returns>
        /// Obiekt <see cref="Task"/>, reprezentujący operację asynchroniczną,
        /// którego zawartością jest obiekt <see cref="Schedule"/> lub <see cref="null"/>,
        /// </returns>
        /// <remarks>
        /// Jeśli harmonogram o podanym identyfikatorze nie istnieje, zwracane jest <see cref="null"/>.
        /// </remarks>
        Task<Schedule?> GetScheduleAsync(int id);

        /// <summary>
        /// Pobiera wszystkie harmonogramy dla aktywnego planu (jeśli istnieje).
        /// </summary>
        /// <returns>
        /// Obiekt <see cref="Task"/>, reprezentujący operację asynchroniczną,
        /// którego zawartością jest lista obiektów <see cref="Schedule"/>.
        /// </returns>
        /// <remarks>
        /// Jeśli nie jest ustawiony żaden aktywny plan, funkcja zwraca pustą listę.
        /// </remarks>
        Task<IEnumerable<Schedule>> GetActiveSchedules();

        /// <summary>
        /// Zapisuje harmonogram w bazie danych.
        /// </summary>
        /// <param name="schedule">Harmonogram, który ma zostać zapisany.</param>
        /// <returns>
        /// Obiekt <see cref="Task"/>, reprezentujący operację asynchroniczną.
        /// </returns>
        Task CreateScheduleAsync(Schedule schedule);

        /// <summary>
        /// Usuwa harmonogram o podanym identyfikatorze.
        /// </summary>
        /// <param name="id">Id harmonogramu, który ma zostać usunięty.</param>
        /// <returns>
        /// Obiekt <see cref="Task"/>, reprezentujący operację asynchroniczną,
        /// którego zawartością jest wartość logiczna określająca, czy usunięcie się powiodło.
        /// </returns>
        Task<bool> DeleteScheduleAsync(int id);

        /// <summary>
        /// Aktualizuje harmonogram w bazie danych.
        /// </summary>
        /// <param name="schedule">Harmonogram, który ma zostać zaktualizowany.</param>
        /// <returns>
        /// Obiekt <see cref="Task"/>, reprezentujący operację asynchroniczną.
        /// </returns>
        Task UpdateScheduleAsync(Schedule schedule);

        /// <summary>
        /// Ustawia domyślny harmonogram dla aktywnego planu (jeśli istnieje).
        /// </summary>
        /// <param name="schedule">Harmonogram, który ma być aktywny.</param>
        /// <returns>
        /// Obiekt <see cref="Task"/>, reprezentujący operację asynchroniczną.
        /// </returns>
        Task SetDefaultScheduleAsync(Schedule schedule);

        /// <summary>
        /// Pobiera domyślny harmonogram dla aktywnego planu (jeśli istnieje).
        /// </summary>
        /// <returns>
        /// Obiekt <see cref="Task"/>, reprezentujący operację asynchroniczną,
        /// którego zawartością jest obiekt <see cref="Schedule"/> lub <see cref="null"/>.
        /// </returns>
        Task<Schedule?> GetDefaultScheduleAsync();

        /// <summary>
        /// Usuwa informację o domyślnym harmonogramie dla aktywnego planu.
        /// </summary>
        /// <returns></returns>
        Task ClearDefaultScheduleAsync();
    }
}