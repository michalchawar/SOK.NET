using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace app.Models
{
    /// <summary>
    /// Reprezentuje harmonogram wizyt w ramach danego planu.
    /// Harmonogram przede wszystkim grupuje wizyty, bêd¹c kategori¹ dla nich.
    /// Dwa podstawowe harmonogramy, tworzone domyœlnie dla ka¿dego planu to:
    /// "W terminie zasadniczym" oraz "W terminie dodatkowym".
    /// Harmonogram okreœla równie¿ charakter powi¹zania budynków z agendami.
    /// </summary>
    public class Schedule
    {
        /// <summary>
        /// Unikalny identyfikator harmonogramu (klucz g³ówny).
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Nazwa harmonogramu.
        /// </summary>
        [Required, MaxLength(128)]
        public string Name { get; set; } = default!;

        /// <summary>
        /// Krótka nazwa harmonogramu. U¿ywana w wiêkszoœci miejsc.
        /// </summary>
        [Required, MaxLength(24)]
        public string ShortName { get; set; } = default!;

        /// <summary>
        /// Identyfikator planu, do którego nale¿y harmonogram.
        /// </summary>
        [Required]
        public int PlanId { get; set; }

        /// <summary>
        /// Plan, do którego nale¿y harmonogram (relacja nawigacyjna).
        /// </summary>
        public Plan Plan { get; set; } = default!;

        /// <summary>
        /// Lista przypisañ budynków do agend w tym harmonogramie. 
        /// To klasa pomocnicza relacji wiele-do-wielu miêdzy agend¹ a budynkami.
        /// </summary>
        public ICollection<BuildingAssignment> BuildingAssignments { get; set; } = new List<BuildingAssignment>();
        /// <summary>
        /// Lista agend powi¹zanych z harmonogramem.
        /// </summary>
        public ICollection<Agenda> Agendas { get; set; } = new List<Agenda>();

        /// <summary>
        /// Lista zg³oszeñ powi¹zanych z harmonogramem.
        /// </summary>
        public ICollection<Submission> Submissions { get; set; } = new List<Submission>();
    }
}