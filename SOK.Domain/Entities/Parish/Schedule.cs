using System.ComponentModel.DataAnnotations;
using SOK.Domain.Interfaces;

namespace SOK.Domain.Entities.Parish
{
    /// <summary>
    /// Reprezentuje harmonogram wizyt w ramach danego planu.
    /// Harmonogram przede wszystkim grupuje wizyty, będąc kategorią dla nich.
    /// Dwa podstawowe harmonogramy, tworzone domyślnie dla każdego planu to:
    /// "W terminie zasadniczym" oraz "W terminie dodatkowym".
    /// Harmonogram określa również charakter powiązania budynków z agendami.
    /// </summary>
    public class Schedule : IEntityMetadata
    {
        /// <summary>
        /// Unikalny identyfikator harmonogramu (klucz główny).
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Nazwa harmonogramu.
        /// </summary>
        [MaxLength(128)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Krótka nazwa harmonogramu. Używana w większości miejsc.
        /// </summary>
        [MaxLength(24)]
        public string ShortName { get; set; } = string.Empty;

        /// <summary>
        /// Identyfikator planu, do którego należy harmonogram.
        /// </summary>
        public int PlanId { get; set; }

        /// <summary>
        /// Plan, do którego należy harmonogram (relacja nawigacyjna).
        /// </summary>
        public Plan Plan { get; set; } = default!;

        /// <summary>
        /// Lista przypisań budynków do agend w tym harmonogramie. 
        /// To klasa pomocnicza relacji wiele-do-wielu między agendą a budynkami.
        /// </summary>
        public ICollection<BuildingAssignment> BuildingAssignments { get; set; } = new List<BuildingAssignment>();

        /// <summary>
        /// Lista agend powiązanych z harmonogramem.
        /// </summary>
        public ICollection<Agenda> Agendas { get; set; } = new List<Agenda>();

        /// <summary>
        /// Lista zgłoszeń powiązanych z harmonogramem.
        /// </summary>
        public ICollection<Submission> Submissions { get; set; } = new List<Submission>();

        /// <summary>
        /// Lista wizyt mających ustawiony ten harmonogram.
        /// Bezpośrednio z wizytami powiązane są zgłoszenia.
        /// </summary>
        public ICollection<Visit> Visits { get; set; } = new List<Visit>();
    }
}