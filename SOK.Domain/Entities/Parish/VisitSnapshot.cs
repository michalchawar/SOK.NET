using SOK.Domain.Enums;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace SOK.Domain.Entities.Parish
{
    /// <summary>
    /// Reprezentuje archiwalny stan wizyty w danym momencie.
    /// Pozwala śledzić historię zmian statusu, daty, przewidywanego czasu oraz autora zmiany.
    /// Obiekt wizyty jest przypisany na stałe do jednego zgłoszenia,
    /// zatem snapshot nie uwzględnia zmian zgłoszenia.
    /// </summary>
    public class VisitSnapshot
    {
        /// <summary>
        /// Unikalny identyfikator snapshotu wizyty (klucz główny).
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Numer porządkowy wizyty w ramach agendy w momencie utworzenia snapshotu.
        /// </summary>
        [Range(1, 300)]
        public int? OrdinalNumber { get; set; }

        /// <summary>
        /// Status wizyty w momencie utworzenia snapshotu.
        /// </summary>
        [DefaultValue(VisitStatus.Unplanned)]
        public VisitStatus Status { get; set; } = VisitStatus.Unplanned;
        
        /// <summary>
        /// Liczba osób przyjmujących wizytę w momencie utworzenia snapshotu.
        /// </summary>
        public int? PeopleCount { get; set; }

        /// <summary>
        /// Id harmonogramu, do którego przypisana była wizyta w momencie utworzenia snapshotu.
        /// Może być null, tylko gdy Status jest równy VisitStatus.Withdrawn.
        /// </summary>
        public int? ScheduleId { get; set; } = null;
        
        /// <summary>
        /// Nazwa harmonogramu, do którego przypisana była wizyta w momencie utworzenia snapshotu.
        /// Może być null, tylko gdy Status jest równy VisitStatus.Withdrawn.
        /// </summary>
        public string? ScheduleName { get; set; } = null;
        
        /// <summary>
        /// Identyfikator agendy, do której przypisana była wizyta w momencie utworzenia snapshotu (opcjonalny).
        /// </summary>
        public int? AgendaId { get; set; }

        /// <summary>
        /// Data wizyty w momencie utworzenia snapshotu (jeśli dotyczy).
        /// </summary>
        public DateOnly? Date { get; set; }

        /// <summary>
        /// Czy data wizyty była widoczna dla użytkownika w momencie utworzenia snapshotu.
        /// </summary>
        public bool? DateVisibility { get; set; }

        /// <summary>
        /// Przewidywany czas wizyty w momencie utworzenia snapshotu (jeśli dotyczy).
        /// </summary>
        public TimeOnly? PredictedTime { get; set; }

        /// <summary>
        /// Czy przewidywany czas wizyty był widoczny dla użytkownika w momencie utworzenia snapshotu.
        /// </summary>
        public bool? PredictedTimeVisibility { get; set; }

        /// <summary>
        /// Data i godzina utworzenia snapshotu.
        /// </summary>
        public DateTime ChangeTime { get; private set; }

        /// <summary>
        /// Identyfikator użytkownika, który wprowadził zmianę, nadpisując dane z tego snapshotu.
        /// </summary>
        public int? ChangeAuthorId { get; set; }

        /// <summary>
        /// Użytkownik, który wprowadził zmianę, nadpisując dane z tego snapshotu (relacja nawigacyjna).
        /// </summary>
        public ParishMember? ChangeAuthor { get; set; } = default!;

        /// <summary>
        /// Identyfikator wizyty, której dotyczy snapshot.
        /// </summary>
        public int VisitId { get; set; }

        /// <summary>
        /// Wizyta, której dotyczy snapshot (relacja nawigacyjna).
        /// </summary>
        public Visit Visit { get; set; } = default!;
    }
}