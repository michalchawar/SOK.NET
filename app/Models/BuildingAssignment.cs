using System.ComponentModel.DataAnnotations;

namespace app.Models
{
    /// <summary>
    /// Reprezentuje przypisanie budynku do agendy i harmonogramu.
    /// Pozwala powi¹zaæ konkretny budynek z agend¹ w ramach danego harmonogramu (Schedule).
    /// Przypisania wykorzystywane s¹ do automatycznego przypisywania wizyt do danych agend
    /// oraz sugerowania agend przy planowaniu wizyt. Aby dane przypisanie by³o wtedy brane pod uwagê,
    /// harmonogram wizyty musi byæ zgodny z harmonogramem przypisania (oraz adres wizyty musi byæ
    /// zgodny z adresem budynku).
    /// </summary>
    public class BuildingAssignment
    {
        /// <summary>
        /// Identyfikator agendy, do której przypisany jest budynek.
        /// </summary>
        [Required]
        public int AgendaId { get; set; }

        /// <summary>
        /// Agenda, do której przypisany jest budynek (relacja nawigacyjna).
        /// </summary>
        public Agenda Agenda { get; set; } = default!;

        /// <summary>
        /// Identyfikator budynku, który jest przypisany.
        /// </summary>
        [Required]
        public int BuildingId { get; set; }

        /// <summary>
        /// Budynek, który jest przypisany do agendy (relacja nawigacyjna).
        /// </summary>
        public Building Building { get; set; } = default!;

        /// <summary>
        /// Identyfikator harmonogramu, w ramach którego nastêpuje przypisanie.
        /// </summary>
        [Required]
        public int ScheduleId { get; set; }

        /// <summary>
        /// Harmonogram, w ramach którego nastêpuje przypisanie budynku (relacja nawigacyjna).
        /// </summary>
        public Schedule Schedule { get; set; } = default!;
    }
}