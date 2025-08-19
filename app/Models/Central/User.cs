using System.ComponentModel.DataAnnotations;

namespace app.Models.Central
{
    /// <summary>
    /// Reprezentuje u¿ytkownika systemu i jego przynale¿noœæ.
    /// Przechowuje dane do identyfikacji (login, e-mail) i unikatowy identyfikator parafii, do której nale¿y.
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
        /// Adres e-mail u¿ytkownika (opcjonalny).
        /// </summary>
        [MaxLength(256)]
        public string? Email { get; set; } = default!;

        /// <summary>
        /// Unikalny identyfikator parafii, do której przypisany jest u¿ytkownik.
        /// </summary>
        [Required]
        public Guid ParishUniqueId { get; set; }
    }
}