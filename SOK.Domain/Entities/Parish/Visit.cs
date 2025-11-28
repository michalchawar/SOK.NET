using SOK.Domain.Enums;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace SOK.Domain.Entities.Parish
{
    /// <summary>
    /// Reprezentuje pojedynczą wizytę duszpasterską w ramach agendy i harmonogramu.
    /// Zawiera informacje o statusie, kolejności, powiązaniach z agendą, harmonogramem oraz historię zmian.
    /// </summary>
    public class Visit
    {
        /// <summary>
        /// Unikalny identyfikator wizyty (klucz główny).
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Numer porządkowy wizyty w ramach agendy (planowana kolejność odwiedzin).
        /// </summary>
        [Range(1, 300)]
        public int? OrdinalNumber { get; set; }

        /// <summary>
        /// Status wizyty. Wizyta nieprzypisana do żadnej agendy ma status Unplanned.
        /// </summary>
        [DefaultValue(VisitStatus.Unplanned)]
        public VisitStatus Status { get; set; } = VisitStatus.Unplanned;

        /// <summary>
        /// Identyfikator agendy, do której przypisana jest wizyta (opcjonalny).
        /// </summary>
        public int? AgendaId { get; set; }

        /// <summary>
        /// Agenda, do której przypisana jest wizyta (relacja opcjonalna).
        /// </summary>
        public Agenda? Agenda { get; set; } = default!;

        /// <summary>
        /// Identyfikator harmonogramu, do którego należy wizyta (opcjonalny).
        /// Może być null, tylko gdy Status jest równy VisitStatus.Withdrawn.
        /// </summary>
        public int? ScheduleId { get; set; }

        /// <summary>
        /// Harmonogram, do którego należy wizyta (relacja nawigacyjna).
        /// Może być null, tylko gdy Status jest równy VisitStatus.Withdrawn.
        /// </summary>
        public Schedule? Schedule { get; set; } = default!;

        /// <summary>
        /// Identyfikator zgłoszenia, do którego należy wizyta.
        /// </summary>
        public int SubmissionId { get; set; }

        /// <summary>
        /// Zgłoszenie, do którego należy wizyta (relacja nawigacyjna).
        /// </summary>
        public Submission Submission { get; set; } = default!;

        /// <summary>
        /// Historia zmian wizyty (snapshoty).
        /// </summary>
        public ICollection<VisitSnapshot> History { get; set; } = new List<VisitSnapshot>();
    }
}