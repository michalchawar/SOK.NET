using SOK.Application.Common.DTO;
using SOK.Domain.Entities.Parish;

namespace SOK.Application.Services.Interface
{
    /// <summary>
    /// Serwis do zarządzania wizytami.
    /// </summary>
    public interface IVisitService
    {
        /// <summary>
        /// Automatycznie przypisuje wizytę do odpowiedniej agendy w danym dniu.
        /// Wybiera agendę według priorytetów:
        /// 1. Agenda z największą liczbą wizyt z tego samego budynku i harmonogramu
        /// 2. Agenda z największą liczbą wizyt z tego samego budynku (dowolny harmonogram)
        /// 3. Agenda z najmniejszą liczbą wizyt
        /// 4. Jeśli nie ma żadnej agendy - tworzy nową
        /// </summary>
        /// <param name="visitId">Identyfikator wizyty do przypisania.</param>
        /// <param name="dayId">Identyfikator dnia, do którego przypisać wizytę.</param>
        /// <param name="sendEmail">Czy wysłać email z powiadomieniem o zaplanowaniu wizyty.</param>
        Task AssignVisitToDay(int visitId, int dayId, bool sendEmail = false);

        /// <summary>
        /// Sprawdza czy dla danego budynku i harmonogramu istnieje automatyczne przypisanie do dnia.
        /// </summary>
        /// <param name="buildingId">Identyfikator budynku.</param>
        /// <param name="scheduleId">Identyfikator harmonogramu.</param>
        /// <returns>Datę dnia jeśli istnieje auto-assign, w przeciwnym razie null.</returns>
        Task<DateOnly?> GetAutoAssignmentDate(int buildingId, int scheduleId);
    }
}
