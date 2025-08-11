using System;
using System.ComponentModel.DataAnnotations;

namespace app.Models
{
    /// <summary>
    /// Reprezentuje archiwalny stan danych osoby zg³aszaj¹cej (Submitter) w danym momencie.
    /// Pozwala œledziæ historiê zmian danych kontaktowych zg³aszaj¹cego.
    /// </summary>
    public class SubmitterSnapshot
    {
        /// <summary>
        /// Unikalny identyfikator snapshotu (klucz g³ówny).
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Publiczny unikalny identyfikator zg³aszaj¹cego w momencie utworzenia snapshotu.
        /// </summary>
        [Required]
        public Guid UniqueId { get; set; } = default!;

        /// <summary>
        /// Imiê osoby zg³aszaj¹cej w momencie utworzenia snapshotu.
        /// </summary>
        [Required, MaxLength(64)]
        public string Name { get; set; } = default!;

        /// <summary>
        /// Nazwisko osoby zg³aszaj¹cej w momencie utworzenia snapshotu.
        /// </summary>
        [Required, MaxLength(64)]
        public string Surname { get; set; } = default!;

        /// <summary>
        /// Adres e-mail osoby zg³aszaj¹cej w momencie utworzenia snapshotu (opcjonalny).
        /// </summary>
        [MaxLength(256)]
        public string? Email { get; set; }

        /// <summary>
        /// Numer telefonu osoby zg³aszaj¹cej w momencie utworzenia snapshotu (opcjonalny).
        /// </summary>
        [MaxLength(15)]
        public string? Phone { get; set; }

        /// <summary>
        /// Data i godzina utworzenia snapshotu.
        /// </summary>
        public DateTime ChangeTime { get; set; }

        /// <summary>
        /// Identyfikator u¿ytkownika, który wprowadzi³ zmianê, nadpisuj¹c dane z tego snapshotu.
        /// </summary>
        [Required]
        public int ChangeAuthorId { get; set; } = default!;

        /// <summary>
        /// U¿ytkownik, który wprowadzi³ zmianê, nadpisuj¹c dane z tego snapshotu (relacja nawigacyjna).
        /// </summary>
        public User ChangeAuthor { get; set; } = default!;
    }
}