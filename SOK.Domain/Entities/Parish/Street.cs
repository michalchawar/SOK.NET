using System.ComponentModel.DataAnnotations;

namespace SOK.Domain.Entities.Parish
{
    /// <summary>
    /// Reprezentuje ulicę w mieście.
    /// Ulica może mieć wiele budynków i jest powiązana z określonym typem (np. ul., al., pl.).
    /// </summary>
    public class Street
    {
        /// <summary>
        /// Unikalny identyfikator ulicy (klucz główny).
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Nazwa ulicy.
        /// </summary>
        [MaxLength(128)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Kod pocztowy przypisany do ulicy (opcjonalny).
        /// </summary>
        [MaxLength(16)]
        public string? PostalCode { get; set; } = null;

        /// <summary>
        /// Identyfikator typu ulicy.
        /// </summary>
        public int StreetSpecifierId { get; set; }

        /// <summary>
        /// Typ ulicy (relacja nawigacyjna).
        /// </summary>
        public StreetSpecifier Type { get; set; } = default!;

        /// <summary>
        /// Identyfikator miasta, w którym znajduje się ulica.
        /// </summary>
        public int CityId { get; set; }

        /// <summary>
        /// Miasto, w którym znajduje się ulica (relacja nawigacyjna).
        /// </summary>
        public City City { get; set; } = default!;

        /// <summary>
        /// Lista budynków znajdujących się przy tej ulicy.
        /// </summary>
        public ICollection<Building> Buildings { get; set; } = new List<Building>();
    }
}