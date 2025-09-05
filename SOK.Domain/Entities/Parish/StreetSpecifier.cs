using System.ComponentModel.DataAnnotations;

namespace SOK.Domain.Entities.Parish
{
    /// <summary>
    /// Reprezentuje typ ulicy (ulica, aleja, plac, itp.) wraz ze skrótem.
    /// Pozwala na rozró¿nienie rodzaju ulicy w adresie.
    /// </summary>
    public class StreetSpecifier
    {
        /// <summary>
        /// Unikalny identyfikator typu ulicy (klucz g³ówny).
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Pe³na nazwa typu ulicy, np. "Ulica", "Aleja", "Plac".
        /// </summary>
        [MaxLength(64)]
        public string FullName { get; set; } = default!;

        /// <summary>
        /// Skrót typu ulicy, np. "ul.", "al.", "pl." (opcjonalny).
        /// </summary>
        [MaxLength(16)]
        public string? Abbreviation { get; set; }
    }
}