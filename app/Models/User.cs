using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using app.Models.Enums;

namespace app.Models
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
        /// Login u¿ytkownika (unikalny w systemie).
        /// </summary>
        [Required, MaxLength(64)]
        public string Login { get; set; } = default!;

        /// <summary>
        /// Nazwa wyœwietlana u¿ytkownika (np. imiê i nazwisko).
        /// </summary>
        [Required, MaxLength(128)]
        public string DisplayName { get; set; } = default!;

        /// <summary>
        /// Adres e-mail u¿ytkownika (opcjonalny).
        /// </summary>
        [MaxLength(256)]
        public string? Email { get; set; } = default!;

        /// <summary>
        /// Hash has³a u¿ytkownika.
        /// </summary>
        [Required, MaxLength(256)]
        public string PasswordHash { get; set; } = default!;

        /// <summary>
        /// Okreœla, czy konto u¿ytkownika jest aktywne w systemie.
        /// </summary>
        [Required, DefaultValue(true)]
        public bool IsActive { get; set; }

        /// <summary>
        /// Zbiór ról przypisanych u¿ytkownikowi.
        /// </summary>
        [Required]
        public HashSet<Role> Roles { get; set; } = new HashSet<Role>();

        /// <summary>
        /// Identyfikator parafii, do której przypisany jest u¿ytkownik.
        /// </summary>
        [Required]
        public int ParishId { get; set; }

        /// <summary>
        /// Parafia, do której przypisany jest u¿ytkownik (relacja nawigacyjna).
        /// </summary>
        public Parish Parish { get; set; } = default!;

        /// <summary>
        /// Lista agend, do których u¿ytkownik jest przypisany (np. jako ksi¹dz lub ministrant).
        /// </summary>
        public ICollection<Agenda> AssignedAgendas { get; set; } = new List<Agenda>();
    }
}