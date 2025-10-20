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
        /// Zg³oszenie powi¹zane z adresem (relacja opcjonalna).
        /// </summary>
        public Submission? Submission { get; set; } = default!;


        /// <summary>
        /// Numer budynku (cache).
        /// </summary>
        /// <remarks>
        /// Aktualizowany jest przez wyzwalacz bazy danych przy ka¿dej zmianie powi¹zanego budynku.
        /// </remarks>
        [Range(1, 300)]
        public int? BuildingNumber { get; private set; }

        /// <summary>
        /// Litera budynku (cache).
        /// </summary>
        /// <remarks>
        /// Aktualizowany jest przez wyzwalacz bazy danych przy ka¿dej zmianie powi¹zanego budynku.
        /// </remarks>
        [MaxLength(3)]
        public string? BuildingLetter { get; private set; }

        /// <summary>
        /// Nazwa ulicy (cache).
        /// </summary>
        /// <remarks>
        /// Aktualizowany jest przez wyzwalacz bazy danych przy ka¿dej zmianie powi¹zanej ulicy.
        /// </remarks>
        [MaxLength(128)]
        public string? StreetName { get; private set; }

        /// <summary>
        /// Skrót typu ulicy (cache).
        /// </summary>
        /// <remarks>
        /// Aktualizowany jest przez wyzwalacz bazy danych przy ka¿dej zmianie powi¹zanej ulicy.
        /// </remarks>
        [MaxLength(64)]
        public string? StreetType { get; private set; }

        /// <summary>
        /// Nazwa miasta (cache).
        /// </summary>
        /// <remarks>
        /// Aktualizowany jest przez wyzwalacz bazy danych przy ka¿dej zmianie powi¹zanego miasta.
        /// </remarks>
        [MaxLength(128)]
        public string? CityName { get; private set; }


        /// <summary>
        /// Reprezentacja tekstowa adresu do celów filtrowania.
        /// </summary>
        /// <remarks>
        /// Aktualizowana jest przez wyzwalacz bazy danych przy ka¿dej zmianie adresu, 
        /// powi¹zanego budynku, ulicy lub miasta.
        /// </remarks>
        [MaxLength(1024)]
        public string FilterableString { get; private set; } = string.Empty;
    }
}