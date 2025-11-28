using System.ComponentModel.DataAnnotations;

namespace SOK.Domain.Entities.Parish
{
    /// <summary>
    /// Reprezentuje miasto, w którym znajdują się ulice i budynki.
    /// Miasto należy do jednej diecezji.
    /// </summary>
    public class City
    {
        /// <summary>
        /// Unikalny identyfikator miasta (klucz główny).
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Nazwa miasta.
        /// </summary>
        [MaxLength(128)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Nazwa wyświetlana miasta (opcjonalna, np. z dodatkowymi informacjami lub skrótami).
        /// Jeśli nie jest ustawiona, używana jest nazwa miasta (Name).
        /// </summary>
        [MaxLength(128)]
        public string? DisplayName { get; set; } = null;

        /// <summary>
        /// Lista ulic znajdujących się w mieście.
        /// </summary>
        public ICollection<Street> Streets { get; set; } = new List<Street>();
    }
}