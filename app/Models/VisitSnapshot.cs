using app.Models.Enums;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace app.Models
{
    /// <summary>
    /// Reprezentuje archiwalny stan wizyty w danym momencie.
    /// Pozwala œledziæ historiê zmian statusu, daty, przewidywanego czasu oraz autora zmiany.
    /// </summary>
    public class VisitSnapshot
    {
        /// <summary>
        /// Unikalny identyfikator snapshotu wizyty (klucz g³ówny).
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Numer porz¹dkowy wizyty w ramach agendy w momencie utworzenia snapshotu.
        /// </summary>
        [Range(1, 300)]
        public short OrdinalNumber { get; set; }

        /// <summary>
        /// Status wizyty w momencie utworzenia snapshotu.
        /// </summary>
        [Required, DefaultValue(VisitStatus.Unplanned)]
        public VisitStatus Status { get; set; }

        /// <summary>
        /// Nazwa harmonogramu, do którego przypisana by³a wizyta w momencie utworzenia snapshotu.
        /// </summary>
        [Required]
        public string ScheduleName { get; set; } = default!;

        /// <summary>
        /// Data wizyty w momencie utworzenia snapshotu (jeœli dotyczy).
        /// </summary>
        public DateTime? Date { get; set; }

        /// <summary>
        /// Czy data wizyty by³a widoczna dla u¿ytkownika w momencie utworzenia snapshotu.
        /// </summary>
        public bool? DateVisibility { get; set; }
        
        /// <summary>
        /// Przewidywany czas wizyty w momencie utworzenia snapshotu (jeœli dotyczy).
        /// </summary>
        public TimeOnly? PredictedTime { get; set; }

        /// <summary>
        /// Czy przewidywany czas wizyty by³ widoczny dla u¿ytkownika w momencie utworzenia snapshotu.
        /// </summary>
        public bool? PredictedTimeVisibility { get; set; }

        /// <summary>
        /// Data i godzina utworzenia snapshotu.
        /// </summary>
        public DateTime ChangeTime { get; set; }

        /// <summary>
        /// Login u¿ytkownika, który wprowadzi³ zmianê, nadpisuj¹c dane z tego snapshotu.
        /// </summary>
        [Required]
        public string ChangeAuthorLogin { get; set; } = default!;

        /// <summary>
        /// U¿ytkownik, który wprowadzi³ zmianê, nadpisuj¹c dane z tego snapshotu (relacja nawigacyjna).
        /// </summary>
        public User ChangeAuthor { get; set; } = default!;

        /// <summary>
        /// Identyfikator wizyty, której dotyczy snapshot.
        /// </summary>
        [Required]
        public int VisitId { get; set; }

        /// <summary>
        /// Wizyta, której dotyczy snapshot (relacja nawigacyjna).
        /// </summary>
        public Visit Visit { get; set; } = default!;
    }
}