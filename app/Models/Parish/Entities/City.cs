using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace app.Models.Parish.Entities
{
    /// <summary>
    /// Reprezentuje miasto, w którym znajduj¹ siê ulice i budynki.
    /// Miasto nale¿y do jednej diecezji.
    /// </summary>
    public class City
    {
        /// <summary>
        /// Unikalny identyfikator miasta (klucz g³ówny).
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Nazwa miasta.
        /// </summary>
        [MaxLength(128)]
        public string Name { get; set; } = default!;

        /// <summary>
        /// Nazwa wyœwietlana miasta (opcjonalna, np. z dodatkowymi informacjami lub skrótami).
        /// Jeœli nie jest ustawiona, u¿ywana jest nazwa miasta (Name).
        /// </summary>
        [MaxLength(128)]
        public string? DisplayName { get; set; }

        /// <summary>
        /// Lista ulic znajduj¹cych siê w mieœcie.
        /// </summary>
        public ICollection<Street> Streets { get; set; } = new List<Street>();
    }

    public class CityEntityTypeConfiguration : IEntityTypeConfiguration<City>
    {
        public void Configure(EntityTypeBuilder<City> builder)
        {
            // Klucz g³ówny
            // (zdefiniowany przez atrybut [Key] w modelu)

            // Indeksy i unikalnoœæ
            // (nie ma potrzeby dodatkowych indeksów poza kluczem g³ównym)

            // Generowane pola
            // (brak automatycznie generowanych pól)

            // Relacje
            // (City nie jest podrzêdne wzglêdem ¿adnej encji, nie konfigurujemy relacji)
        }
    }
}