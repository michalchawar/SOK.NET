using System.ComponentModel.DataAnnotations;

namespace SOK.Domain.Entities.Parish
{
    /// <summary>
    /// Reprezentuje plan wizyt duszpasterskich dla danej parafii.
    /// Plan zawiera informacje o autorze, parafii, harmonogramach, zgłoszeniach oraz dniach wizyt.
    /// </summary>
    public class Plan
    {
        /// <summary>
        /// Unikalny identyfikator planu (klucz główny).
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Nazwa planu.
        /// </summary>
        [MinLength(4)]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Data i godzina utworzenia planu.
        /// </summary>
        public DateTime CreationTime { get; private set; }

        /// <summary>
        /// Identyfikator domyślnego harmonogramu dla planu.
        /// </summary>
        public int? DefaultScheduleId { get; set; }

        /// <summary>
        /// Domyślny harmonogram (Schedule) powiązany z planem (relacja nawigacyjna).
        /// Odpowiada za możliwość przyjmowania zgłoszeń przez formularz zewnętrzny.
        /// </summary>
        /// <remarks>
        /// Może być <see cref="null">, jeśli nie ustawiono domyślnego harmonogramu.
        /// </remarks>
        public Schedule? DefaultSchedule { get; set; } = default!;

        /// <summary>
        /// Identyfikator użytkownika, który jest autorem planu.
        /// </summary>
        public int? AuthorId { get; set; }

        /// <summary>
        /// Autor planu (relacja opcjonalna).
        /// </summary>
        public ParishMember? Author { get; set; } = default!;

        /// <summary>
        /// Lista księży (ParishMember w roli Priest), których można wybierać w planie.
        /// </summary>
        public ICollection<ParishMember> ActivePriests { get; set; } = new List<ParishMember>();

        /// <summary>
        /// Lista harmonogramów (Schedule) powiązanych z planem.
        /// </summary>
        public ICollection<Schedule> Schedules { get; set; } = new List<Schedule>();

        /// <summary>
        /// Lista zgłoszeń (Submission) powiązanych z planem.
        /// </summary>
        public ICollection<Submission> Submissions { get; set; } = new List<Submission>();

        /// <summary>
        /// Lista dni (Day) w ramach planu.
        /// </summary>
        public ICollection<Day> Days { get; set; } = new List<Day>();
    }
}