using System.ComponentModel.DataAnnotations;

namespace SOK.Domain.Entities.Parish
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
        public int StreetSpecifierId { get; set; }

        /// <summary>
        /// Typ ulicy (relacja nawigacyjna).
        /// </summary>
        public StreetSpecifier Type { get; set; } = default!;

        /// <summary>
        /// Identyfikator miasta, w którym znajduje siê ulica.
        /// </summary>
        public int CityId { get; set; }

        /// <summary>
        /// Miasto, w którym znajduje siê ulica (relacja nawigacyjna).
        /// </summary>
        public City City { get; set; } = default!;

        /// <summary>
        /// Lista budynków znajduj¹cych siê przy tej ulicy.
        /// </summary>
        public ICollection<Building> Buildings { get; set; } = new List<Building>();
    }
}