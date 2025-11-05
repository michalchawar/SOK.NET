using System.ComponentModel.DataAnnotations;

namespace SOK.Domain.Entities.Parish
{
    /// <summary>
    /// Reprezentuje u¿ytkownika systemu (np. administratora, ksiêdza, ministranta).
    /// Przechowuje dane logowania, status, role oraz powi¹zania z parafi¹ i agendami.
    /// </summary>
    public class ParishMember
    {
        /// <summary>
        /// Unikalny identyfikator u¿ytkownika (klucz g³ówny).
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Nazwa wyœwietlana u¿ytkownika.
        /// </summary>
        [MaxLength(64)]
        public string DisplayName { get; set; } = default!;

        /// <summary>
        /// Identyfikator odpowiadaj¹cego u¿ytkownika w bazie centralnej.
        /// </summary>
        [MaxLength(36)]
        public string CentralUserId { get; set; } = default!;

        /// <summary>
        /// Lista agend, do których u¿ytkownik jest przypisany (np. jako ksi¹dz lub ministrant).
        /// </summary>
        public ICollection<Agenda> AssignedAgendas { get; set; } = new List<Agenda>();

        /// <summary>
        /// Lista zg³oszeñ, które u¿ytkownik wprowadzi³ manualnie.
        /// </summary>
        public ICollection<FormSubmission> EnteredSubmissions { get; set; } = new List<FormSubmission>();
    }
}