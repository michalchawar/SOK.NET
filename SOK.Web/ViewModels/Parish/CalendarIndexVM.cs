using System.ComponentModel.DataAnnotations;

namespace SOK.Web.ViewModels.Parish
{
    /// <summary>
    /// ViewModel dla głównego widoku kalendarza.
    /// </summary>
    public class CalendarIndexVM
    {
        /// <summary>
        /// Identyfikator aktywnego planu.
        /// </summary>
        public int PlanId { get; set; }

        /// <summary>
        /// Nazwa aktywnego planu.
        /// </summary>
        public string PlanName { get; set; } = string.Empty;

        /// <summary>
        /// Data rozpoczęcia kolędy.
        /// </summary>
        public DateTime? VisitsStartDate { get; set; }

        /// <summary>
        /// Data zakończenia kolędy.
        /// </summary>
        public DateTime? VisitsEndDate { get; set; }

        /// <summary>
        /// Lista dni kolędowych.
        /// </summary>
        public List<DayItemVM> Days { get; set; } = new();

        /// <summary>
        /// Czy plan ma jakiekolwiek dni zdefiniowane.
        /// </summary>
        public bool HasDays => Days.Any();
    }

    /// <summary>
    /// ViewModel dla pojedynczego dnia w widoku kalendarza.
    /// </summary>
    public class DayItemVM
    {
        /// <summary>
        /// Identyfikator dnia.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Data dnia.
        /// </summary>
        public DateOnly Date { get; set; }

        /// <summary>
        /// Godzina rozpoczęcia.
        /// </summary>
        public TimeOnly StartHour { get; set; }

        /// <summary>
        /// Godzina zakończenia.
        /// </summary>
        public TimeOnly EndHour { get; set; }

        /// <summary>
        /// Harmonogramy z przypisanymi ulicami dla tego dnia.
        /// </summary>
        public List<ScheduleStreetsVM> Schedules { get; set; } = new();

        /// <summary>
        /// Czy dzień ma jakiekolwiek przypisania.
        /// </summary>
        public bool HasAssignments => Schedules.Any();
    }

    /// <summary>
    /// ViewModel dla harmonogramu z przypisanymi ulicami.
    /// </summary>
    public class ScheduleStreetsVM
    {
        /// <summary>
        /// Identyfikator harmonogramu.
        /// </summary>
        public int ScheduleId { get; set; }

        /// <summary>
        /// Nazwa harmonogramu.
        /// </summary>
        public string ScheduleName { get; set; } = string.Empty;

        /// <summary>
        /// Kolor harmonogramu (hex).
        /// </summary>
        public string ScheduleColor { get; set; } = string.Empty;

        /// <summary>
        /// Lista nazw ulic przypisanych w tym harmonogramie (alfabetycznie).
        /// </summary>
        public List<string> StreetNames { get; set; } = new();
    }
}
