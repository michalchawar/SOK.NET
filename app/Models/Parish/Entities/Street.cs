using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace app.Models.Parish.Entities
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

    public class StreetEntityTypeConfiguration : IEntityTypeConfiguration<Street>
    {
        public void Configure(EntityTypeBuilder<Street> builder)
        {
            // Klucz g³ówny
            // (zdefiniowany przez atrybut [Key] w modelu)

            // Indeksy i unikalnoœæ
            builder.HasIndex(s => s.Name)
                .IsUnique();

            // Generowane pola
            // (brak automatycznie generowanych pól)

            // Relacje
            builder.HasOne(s => s.Type)
                .WithMany()
                .HasForeignKey(s => s.StreetSpecifierId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(s => s.City)
                .WithMany(c => c.Streets)
                .HasForeignKey(s => s.CityId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}