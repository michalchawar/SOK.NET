using System.ComponentModel.DataAnnotations;

namespace SOK.Domain.Entities.Parish
{
    /// <summary>
    /// Reprezentuje adres fizyczny.
    /// </summary>
    public class Address
    {
        /// <summary>
        /// Unikalny identyfikator adresu (klucz g³ówny).
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Numer apartamentu/mieszkania (jeœli w budynku jest wiele).
        /// Krotka (Building, ApartmentNumber, ApartmentLetter) jest unikalna.
        /// </summary>
        [Range(1, 300)]
        public int? ApartmentNumber { get; set; }

        /// <summary>
        /// Litera apartamentu/mieszkania (opcjonalna).
        /// </summary>
        [MaxLength(3)]
        public string? ApartmentLetter { get; set; }

        /// <summary>
        /// Identyfikator budynku, w którym znajduje siê adres.
        /// </summary>
        public int BuildingId { get; set; }

        /// <summary>
        /// Budynek, w którym znajduje siê adres (relacja nawigacyjna).
        /// </summary>
        public Building Building { get; set; } = default!;

        /// <summary>
        /// Identyfikator ulicy, na której znajduje siê adres.
        /// </summary>
        public int StreetId { get; set; }

        /// <summary>
        /// Ulica, na której znajduje siê adres (relacja nawigacyjna).
        /// </summary>
        public Street Street { get; set; } = default!;

        /// <summary>
        /// Identyfikator miasta, w którym znajduje siê adres.
        /// </summary>
        public int CityId { get; set; }

        /// <summary>
        /// Miasto, w którym znajduje siê adres (relacja nawigacyjna).
        /// </summary>
        public City City { get; set; } = default!;

        /// <summary>
        /// Zg³oszenie powi¹zane z adresem (relacja opcjonalna).
        /// </summary>
        public Submission? Submission { get; set; } = default!;
    }
}