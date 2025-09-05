using SOK.Domain.Enums;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace SOK.Domain.Entities.Parish
{
    /// <summary>
    /// Reprezentuje pojedyncz¹ wizytê duszpastersk¹ w ramach agendy i harmonogramu.
    /// Zawiera informacje o statusie, kolejnoœci, powi¹zaniach z agend¹, harmonogramem oraz historiê zmian.
    /// </summary>
    public class Visit
    {
        /// <summary>
        /// Unikalny identyfikator wizyty (klucz g³ówny).
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Numer porz¹dkowy wizyty w ramach agendy (planowana kolejnoœæ odwiedzin).
        /// </summary>
        [Range(1, 300)]
        public int? OrdinalNumber { get; set; }

        /// <summary>
        /// Status wizyty. Wizyta nieprzypisana do ¿adnej agendy ma status Unplanned.
        /// </summary>
        [DefaultValue(VisitStatus.Unplanned)]
        public VisitStatus Status { get; set; }

        /// <summary>
        /// Identyfikator agendy, do której przypisana jest wizyta (opcjonalny).
        /// </summary>
        public int? AgendaId { get; set; } = default!;

        /// <summary>
        /// Agenda, do której przypisana jest wizyta (relacja opcjonalna).
        /// </summary>
        public Agenda? Agenda { get; set; } = default!;

        /// <summary>
        /// Identyfikator harmonogramu, do którego nale¿y wizyta (opcjonalny).
        /// Mo¿e byæ null, tylko gdy Status jest równy VisitStatus.Withdrawn.
        /// </summary>
        public int? ScheduleId { get; set; }

        /// <summary>
        /// Harmonogram, do którego nale¿y wizyta (relacja nawigacyjna).
        /// Mo¿e byæ null, tylko gdy Status jest równy VisitStatus.Withdrawn.
        /// </summary>
        public Schedule? Schedule { get; set; } = default!;

        /// <summary>
        /// Identyfikator zg³oszenia, do którego nale¿y wizyta.
        /// </summary>
        public int SubmissionId { get; set; }

        /// <summary>
        /// Zg³oszenie, do którego nale¿y wizyta (relacja nawigacyjna).
        /// </summary>
        public Submission Submission { get; set; } = default!;

        /// <summary>
        /// Historia zmian wizyty (snapshoty).
        /// </summary>
        public ICollection<VisitSnapshot> History { get; set; } = new List<VisitSnapshot>();
    }
}