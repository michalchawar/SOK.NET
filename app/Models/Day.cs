using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace app.Models
{
    /// <summary>
    /// Reprezentuje pojedynczy dzieñ w planie wizyt.
    /// Okreœla datê oraz domyœlne godziny rozpoczêcia i zakoñczenia wizyt.
    /// </summary>
    public class Day
    {
        /// <summary>
        /// Unikalny identyfikator dnia (klucz g³ówny).
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Data, której dotyczy dzieñ.
        /// </summary>
        [Required]
        public DateOnly Date { get; set; }

        /// <summary>
        /// Domyœlna godzina rozpoczêcia wizyt w tym dniu. Mo¿e byæ nadpisywana przez agendy.
        /// </summary>
        [Required]
        public TimeOnly StartHour { get; set; }

        /// <summary>
        /// Domyœlna godzina zakoñczenia wizyt w tym dniu. Mo¿e byæ nadpisywana przez agendy.
        /// </summary>
        [Required]
        public TimeOnly EndHour { get; set; }

        /// <summary>
        /// Identyfikator planu, do którego nale¿y ten dzieñ.
        /// </summary>
        [Required]
        public int PlanId { get; set; }

        /// <summary>
        /// Plan, do którego nale¿y ten dzieñ (relacja nawigacyjna).
        /// </summary>
        public Plan Plan { get; set; } = default!;

        /// <summary>
        /// Lista agend przypisanych do tego dnia.
        /// </summary>
        public ICollection<Agenda> Agendas { get; set; } = new List<Agenda>();
    }
}