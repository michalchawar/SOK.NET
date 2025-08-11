using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace app.Models
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
        [Required]
        public int BuildingId { get; set; }

        /// <summary>
        /// Budynek, w którym znajduje siê adres (relacja nawigacyjna).
        /// </summary>
        public Building Building { get; set; } = default!;

        /// <summary>
        /// Identyfikator ulicy, na której znajduje siê adres.
        /// </summary>
        [Required]
        public int StreetId { get; private set; }

        /// <summary>
        /// Ulica, na której znajduje siê adres (relacja nawigacyjna).
        /// </summary>
        public Street Street { get; private set; } = default!;

        /// <summary>
        /// Identyfikator miasta, w którym znajduje siê adres.
        /// </summary>
        [Required]
        public int CityId { get; private set; }

        /// <summary>
        /// Miasto, w którym znajduje siê adres (relacja nawigacyjna).
        /// </summary>
        public City City { get; private set; } = default!;

        /// <summary>
        /// Identyfikator diecezji, w której znajduje siê adres.
        /// </summary>
        [Required]
        public int DioceseId { get; private set; }

        /// <summary>
        /// Diecezja, w której znajduje siê adres (relacja nawigacyjna).
        /// </summary>
        public Diocese Diocese { get; private set; } = default!;

        /// <summary>
        /// Zg³oszenie powi¹zane z adresem (relacja opcjonalna).
        /// </summary>
        public Submission? Submission { get; set; } = default!;
    }
}