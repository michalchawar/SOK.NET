using System.ComponentModel.DataAnnotations;

namespace SOK.Domain.Entities.Parish
{
    /// <summary>
    /// Reprezentuje u¿ytkownika systemu (np. administratora, ksiêdza, ministranta).
    /// Przechowuje dane logowania, status, role oraz powi¹zania z parafi¹ i agendami.
    /// </summary>
    public class User
    {
        /// <summary>
        /// Unikalny identyfikator u¿ytkownika (klucz g³ówny).
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Nazwa (login) u¿ytkownika (unikalny w systemie).
        /// </summary>
        [MaxLength(64)]
        public string Username { get; set; } = default!;

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