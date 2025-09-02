using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace app.Models.Parish.Entities
{
    /// <summary>
    /// Reprezentuje jednostkę informacji o parafii.
    /// W krotce (Name, Value) przechowywane są nazwa informacji oraz jej wartość.
    /// </summary>
    public class ParishInfo
    {
        /// <summary>
        /// Identyfikator jednostki informacyjnej (klucz główny).
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Nazwa jednostki informacyjnej (np. "UniqueId", "FullName", "BuildingNumber").
        /// </summary>
        [MaxLength(96)]
        public string Name { get; set; } = default!;

        /// <summary>
        /// Wartość jednostki informacyjnej.
        /// </summary>
        [MaxLength(1024)]
        public string Value { get; set; } = default!;
    }

    public class ParishInfoEntityTypeConfiguration : IEntityTypeConfiguration<ParishInfo>
    {
        public void Configure(EntityTypeBuilder<ParishInfo> builder)
        {
            // Klucz główny
            // (zdefiniowany przez atrybut [Key] w modelu)

            // Indeksy i unikalność
            builder.HasIndex(p => p.Name)
                .IsUnique();

            // Generowane pola
            // (brak automatycznie generowanych pól)

            // Relacje
            // (ParishInfo nie jest podrzędne względem żadnej encji, nie konfigurujemy relacji)
        }
    }
}