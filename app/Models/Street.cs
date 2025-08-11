using System.ComponentModel.DataAnnotations;

namespace app.Models
{
    /// <summary>
    /// Reprezentuje ulicê w mieœcie.
    /// Ulica mo¿e mieæ wiele budynków i jest powi¹zana z okreœlonym typem (np. ul., al., pl.).
    /// </summary>
    public class Street
    {
        /// <summary>
        /// Unikalny identyfikator ulicy (klucz g³ówny).
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Nazwa ulicy.
        /// </summary>
        [Required]
        [MaxLength(128)]
        public string Name { get; set; } = default!;

        /// <summary>
        /// Kod pocztowy przypisany do ulicy (opcjonalny).
        /// </summary>
        [MaxLength(16)]
        public string? PostalCode { get; set; }

        /// <summary>
        /// Identyfikator typu ulicy.
        /// </summary>
        [Required]
        public int StreetSpecifierId { get; set; }

        /// <summary>
        /// Typ ulicy (relacja nawigacyjna).
        /// </summary>
        public StreetSpecifier Type { get; set; } = default!;

        /// <summary>
        /// Lista budynków znajduj¹cych siê przy tej ulicy.
        /// </summary>
        public ICollection<Building> Buildings { get; set; } = new List<Building>();
    }
}