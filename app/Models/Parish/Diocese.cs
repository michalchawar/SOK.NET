using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace app.Models.Parish
{
    /// <summary>
    /// Reprezentuje diecezjê, czyli jednostkê administracyjn¹ Koœcio³a.
    /// Diecezja grupuje parafie w danym regionie.
    /// </summary>
    public class Diocese
    {
        /// <summary>
        /// Unikalny identyfikator diecezji (klucz g³ówny).
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Nazwa diecezji.
        /// </summary>
        [Required]
        [MaxLength(128)]
        public string Name { get; set; } = default!;

        /// <summary>
        /// Nazwa wyœwietlana diecezji (opcjonalna, np. z dodatkowymi informacjami lub skrótami).
        /// </summary>
        [MaxLength(128)]
        public string? DisplayName { get; set; }

        /// <summary>
        /// Lista parafii nale¿¹cych do diecezji.
        /// </summary>
        public ICollection<Parish> Parishes { get; set; } = new List<Parish>();
    }
}