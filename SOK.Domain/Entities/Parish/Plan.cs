using System.ComponentModel.DataAnnotations;

namespace SOK.Domain.Entities.Parish
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
        /// Nazwa planu.
        /// </summary>
        [MinLength(4)]
        [MaxLength(100)]
        public string Name { get; set; } = default!;

        /// <summary>
        /// Data i godzina utworzenia planu.
        /// </summary>
        public DateTime CreationTime { get; private set; }

        /// <summary>
        /// Identyfikator domyœlnego harmonogramu dla planu.
        /// </summary>
        public int? DefaultScheduleId { get; set; }

        /// <summary>
        /// Domyœlny harmonogram (Schedule) powi¹zany z planem (relacja nawigacyjna).
        /// Odpowiada za mo¿liwoœæ przyjmowania zg³oszeñ przez formularz zewnêtrzny.
        /// </summary>
        /// <remarks>
        /// Mo¿e byæ <see cref="null">, jeœli nie ustawiono domyœlnego harmonogramu.
        /// </remarks>
        public Schedule? DefaultSchedule { get; set; } = default!;

        /// <summary>
        /// Identyfikator u¿ytkownika, który jest autorem planu.
        /// </summary>
        public int? AuthorId { get; set; }

        /// <summary>
        /// Autor planu (relacja opcjonalna).
        /// </summary>
        public ParishMember? Author { get; set; } = default!;

        /// <summary>
        /// Lista ksiê¿y (ParishMember w roli Priest), których mo¿na wybieraæ w planie.
        /// </summary>
        public ICollection<ParishMember> ActivePriests { get; set; } = new List<ParishMember>();

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