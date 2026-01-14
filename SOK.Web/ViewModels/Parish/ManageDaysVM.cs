using System.ComponentModel.DataAnnotations;

namespace SOK.Web.ViewModels.Parish
{
    /// <summary>
    /// ViewModel dla zarządzania dniami kolędowymi (bulk create/edit).
    /// </summary>
    public class ManageDaysVM
    {
        /// <summary>
        /// Identyfikator planu.
        /// </summary>
        public int PlanId { get; set; }

        /// <summary>
        /// Nazwa planu.
        /// </summary>
        public string PlanName { get; set; } = string.Empty;

        /// <summary>
        /// Data rozpoczęcia kolędy.
        /// </summary>
        [Required(ErrorMessage = "Data rozpoczęcia jest wymagana.")]
        [Display(Name = "Data rozpoczęcia kolędy")]
        public DateTime VisitsStartDate { get; set; } = DateTime.Today;

        /// <summary>
        /// Data zakończenia kolędy.
        /// </summary>
        [Required(ErrorMessage = "Data zakończenia jest wymagana.")]
        [Display(Name = "Data zakończenia kolędy")]
        public DateTime VisitsEndDate { get; set; } = DateTime.Today.AddDays(14);

        /// <summary>
        /// Lista istniejących dni (tylko te z bazy danych).
        /// </summary>
        public List<ManageDayItemVM> ExistingDays { get; set; } = new();

        /// <summary>
        /// Domyślne godziny dla różnych dni tygodnia.
        /// </summary>
        public DefaultHoursVM DefaultHours { get; set; } = new();
    }

    /// <summary>
    /// ViewModel dla domyślnych godzin kolędy.
    /// </summary>
    public class DefaultHoursVM
    {
        /// <summary>
        /// Domyślna godzina rozpoczęcia dla dni roboczych (pn-pt).
        /// </summary>
        public TimeOnly WeekdaysStart { get; set; } = new TimeOnly(16, 0);

        /// <summary>
        /// Domyślna godzina zakończenia dla dni roboczych (pn-pt).
        /// </summary>
        public TimeOnly WeekdaysEnd { get; set; } = new TimeOnly(20, 0);

        /// <summary>
        /// Domyślna godzina rozpoczęcia dla soboty.
        /// </summary>
        public TimeOnly SaturdayStart { get; set; } = new TimeOnly(10, 0);

        /// <summary>
        /// Domyślna godzina zakończenia dla soboty.
        /// </summary>
        public TimeOnly SaturdayEnd { get; set; } = new TimeOnly(18, 0);

        /// <summary>
        /// Domyślna godzina rozpoczęcia dla niedzieli.
        /// </summary>
        public TimeOnly SundayStart { get; set; } = new TimeOnly(14, 0);

        /// <summary>
        /// Domyślna godzina zakończenia dla niedzieli.
        /// </summary>
        public TimeOnly SundayEnd { get; set; } = new TimeOnly(18, 0);
    }

    /// <summary>
    /// ViewModel dla pojedynczego dnia w widoku zarządzania.
    /// </summary>
    public class ManageDayItemVM
    {
        /// <summary>
        /// Identyfikator dnia.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Data dnia.
        /// </summary>
        [Required]
        public DateOnly Date { get; set; }

        /// <summary>
        /// Godzina rozpoczęcia kolędy.
        /// </summary>
        [Required(ErrorMessage = "Godzina rozpoczęcia jest wymagana.")]
        public TimeOnly StartHour { get; set; }

        /// <summary>
        /// Godzina zakończenia kolędy.
        /// </summary>
        [Required(ErrorMessage = "Godzina zakończenia jest wymagana.")]
        public TimeOnly EndHour { get; set; }

        /// <summary>
        /// Dzień tygodnia.
        /// </summary>
        public DayOfWeek DayOfWeek => Date.DayOfWeek;

        /// <summary>
        /// Nazwa dnia tygodnia (po polsku).
        /// </summary>
        public string DayOfWeekName => DayOfWeek switch
        {
            DayOfWeek.Monday => "Poniedziałek",
            DayOfWeek.Tuesday => "Wtorek",
            DayOfWeek.Wednesday => "Środa",
            DayOfWeek.Thursday => "Czwartek",
            DayOfWeek.Friday => "Piątek",
            DayOfWeek.Saturday => "Sobota",
            DayOfWeek.Sunday => "Niedziela",
            _ => ""
        };

        /// <summary>
        /// Krótka nazwa dnia tygodnia.
        /// </summary>
        public string DayOfWeekShort => DayOfWeek switch
        {
            DayOfWeek.Monday => "Pn",
            DayOfWeek.Tuesday => "Wt",
            DayOfWeek.Wednesday => "Śr",
            DayOfWeek.Thursday => "Cz",
            DayOfWeek.Friday => "Pt",
            DayOfWeek.Saturday => "So",
            DayOfWeek.Sunday => "Nd",
            _ => ""
        };
    }
}
