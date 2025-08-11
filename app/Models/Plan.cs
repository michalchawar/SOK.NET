using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace app.Models
{
    /// <summary>
    /// Reprezentuje plan wizyt duszpasterskich dla danej parafii.
    /// Plan zawiera informacje o autorze, parafii, harmonogramach, zg³oszeniach oraz dniach wizyt.
    /// </summary>
    public class Plan
    {
        /// <summary>
        /// Unikalny identyfikator planu (klucz g³ówny).
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Data i godzina utworzenia planu.
        /// </summary>
        [Required]
        public DateTime CreationTime { get; set; }

        /// <summary>
        /// Identyfikator u¿ytkownika, który jest autorem planu.
        /// </summary>
        [Required]
        public int AuthorId { get; set; }

        /// <summary>
        /// Autor planu (relacja nawigacyjna).
        /// </summary>
        public User Author { get; set; } = default!;

        /// <summary>
        /// Identyfikator parafii, do której nale¿y plan.
        /// </summary>
        [Required]
        public string ParishId { get; set; } = default!;

        /// <summary>
        /// Parafia, do której nale¿y plan (relacja nawigacyjna).
        /// </summary>
        public Parish Parish { get; set; } = default!;

        /// <summary>
        /// Lista harmonogramów (Schedule) powi¹zanych z planem.
        /// </summary>
        public ICollection<Schedule> Schedules { get; set; } = new List<Schedule>();

        /// <summary>
        /// Lista zg³oszeñ (Submission) powi¹zanych z planem.
        /// </summary>
        public ICollection<Submission> Submissions { get; set; } = new List<Submission>();

        /// <summary>
        /// Lista dni (Day) w ramach planu.
        /// </summary>
        public ICollection<Day> Days { get; set; } = new List<Day>();
    }
}