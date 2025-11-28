using System.ComponentModel.DataAnnotations;

namespace SOK.Domain.Entities.Parish
{
    /// <summary>
    /// Reprezentuje pojedynczy dzień w planie wizyt.
    /// Określa datę oraz domyślne godziny rozpoczęcia i zakończenia wizyt.
    /// </summary>
    public class Day
    {
        /// <summary>
        /// Unikalny identyfikator dnia (klucz główny).
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Data, której dotyczy dzień.
        /// </summary>
        public DateOnly Date { get; set; }

        /// <summary>
        /// Domyślna godzina rozpoczęcia wizyt w tym dniu. Może być nadpisywana przez agendy.
        /// </summary>
        public TimeOnly StartHour { get; set; }

        /// <summary>
        /// Domyślna godzina zakończenia wizyt w tym dniu. Może być nadpisywana przez agendy.
        /// </summary>
        public TimeOnly EndHour { get; set; }

        /// <summary>
        /// Identyfikator planu, do którego należy ten dzień.
        /// </summary>
        public int PlanId { get; set; }

        /// <summary>
        /// Plan, do którego należy ten dzień (relacja nawigacyjna).
        /// </summary>
        public Plan Plan { get; set; } = default!;

        /// <summary>
        /// Lista przypisań budynków powiązanych z dniem. To klasa pomocnicza relacji wiele-do-wielu między dniem a budynkami.
        /// </summary>
        public ICollection<BuildingAssignment> BuildingAssignments { get; set; } = new List<BuildingAssignment>();

        /// <summary>
        /// Lista budynków przypisanych do dnia. Każdy budynek może być przypisany do wielu dni w różnych harmonogramach.
        /// </summary>
        public ICollection<Building> BuildingsAssigned { get; set; } = new List<Building>();

        /// <summary>
        /// Lista agend przypisanych do tego dnia.
        /// </summary>
        public ICollection<Agenda> Agendas { get; set; } = new List<Agenda>();
    }
}