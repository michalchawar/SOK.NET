using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
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

    public class AddressEntityTypeConfiguration : IEntityTypeConfiguration<Address>
    {
        public void Configure(EntityTypeBuilder<Address> builder)
        {
            // Klucz g³ówny
            // (zdefiniowany przez atrybut [Key] w modelu)

            // Indeksy i unikalnoœæ
            builder.HasIndex(a => new { a.BuildingId, a.ApartmentNumber, a.ApartmentLetter })
                .IsUnique();

            // Generowane pola
            // (brak automatycznie generowanych pól)

            // Relacje
            builder.HasOne(a => a.Building)
                .WithMany(b => b.Addresses)
                .HasForeignKey(a => a.BuildingId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(a => a.Street)
                .WithMany()
                .HasForeignKey(a => a.StreetId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(a => a.City)
                .WithMany()
                .HasForeignKey(a => a.CityId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}