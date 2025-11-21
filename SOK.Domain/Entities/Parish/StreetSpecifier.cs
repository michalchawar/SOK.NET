using System.ComponentModel.DataAnnotations;

namespace SOK.Domain.Entities.Parish
{
    /// <summary>
    /// Reprezentuje typ ulicy (ulica, aleja, plac, itp.) wraz ze skrótem.
    /// Pozwala na rozróżnienie rodzaju ulicy w adresie.
    /// </summary>
    public class StreetSpecifier
    {
        /// <summary>
        /// Unikalny identyfikator typu ulicy (klucz główny).
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Pełna nazwa typu ulicy, np. "Ulica", "Aleja", "Plac".
        /// </summary>
        [MaxLength(64)]
        public string FullName { get; set; } = string.Empty;

        /// <summary>
        /// Skrót typu ulicy, np. "ul.", "al.", "pl." (opcjonalny).
        /// </summary>
        [MaxLength(16)]
        public string? Abbreviation { get; set; } = null;
    }
}