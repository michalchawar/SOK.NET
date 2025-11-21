using System.ComponentModel.DataAnnotations;

namespace SOK.Domain.Entities.Parish
{
    /// <summary>
    /// Reprezentuje użytkownika systemu (np. administratora, księdza, ministranta).
    /// Przechowuje dane logowania, status, role oraz powiązania z parafią i agendami.
    /// </summary>
    public class ParishMember
    {
        /// <summary>
        /// Unikalny identyfikator użytkownika (klucz główny).
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Nazwa wyświetlana użytkownika.
        /// </summary>
        [MaxLength(64)]
        public string DisplayName { get; set; } = string.Empty;

        /// <summary>
        /// Identyfikator odpowiadającego użytkownika w bazie centralnej.
        /// </summary>
        [MaxLength(36)]
        public string CentralUserId { get; set; } = string.Empty;

        /// <summary>
        /// Lista agend, do których użytkownik jest przypisany (np. jako ksiądz lub ministrant).
        /// </summary>
        public ICollection<Agenda> AssignedAgendas { get; set; } = new List<Agenda>();

        /// <summary>
        /// Lista planów, do których użytkownik jest przypisany (jeśli jest w randzie Priest).
        /// </summary>
        public ICollection<Plan> AssignedPlans { get; set; } = new List<Plan>();

        /// <summary>
        /// Lista zgłoszeń, które użytkownik wprowadził manualnie.
        /// </summary>
        public ICollection<FormSubmission> EnteredSubmissions { get; set; } = new List<FormSubmission>();
    }
}