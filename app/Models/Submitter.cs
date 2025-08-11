using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace app.Models
{
    /// <summary>
    /// Reprezentuje osobê zg³aszaj¹c¹.
    /// Przechowuje podstawowe dane kontaktowe oraz powi¹zania ze zg³oszeniami i histori¹ zmian.
    /// </summary>
    public class Submitter
    {
        /// <summary>
        /// Unikalny identyfikator osoby zg³aszaj¹cej (klucz g³ówny).
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Publiczny unikalny identyfikator osoby zg³aszaj¹cej (GUID).
        /// </summary>
        [Required]
        public Guid UniqueId { get; set; } = default!;

        /// <summary>
        /// Imiê osoby zg³aszaj¹cej.
        /// </summary>
        [Required, MaxLength(64)]
        public string Name { get; set; } = default!;

        /// <summary>
        /// Nazwisko osoby zg³aszaj¹cej.
        /// </summary>
        [Required, MaxLength(64)]
        public string Surname { get; set; } = default!;

        /// <summary>
        /// Adres e-mail osoby zg³aszaj¹cej (opcjonalny).
        /// </summary>
        [MaxLength(256)]
        public string? Email { get; set; }

        /// <summary>
        /// Numer telefonu osoby zg³aszaj¹cej (opcjonalny).
        /// </summary>
        [MaxLength(15)]
        public string? Phone { get; set; }

        /// <summary>
        /// Lista zg³oszeñ powi¹zanych z osob¹ zg³aszaj¹c¹.
        /// </summary>
        public ICollection<Submission> Submissions { get; set; } = new List<Submission>();

        /// <summary>
        /// Historia zmian danych osoby zg³aszaj¹cej (snapshoty).
        /// </summary>
        public ICollection<SubmitterSnapshot> History { get; set; } = new List<SubmitterSnapshot>();
    }
}