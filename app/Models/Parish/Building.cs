using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace app.Models.Parish
{
    /// <summary>
    /// Reprezentuje budynek, który mo¿e byæ przypisany do parafii i zawieraæ wiele adresów oraz mieszkañ.
    /// </summary>
    public class Building
    {
        /// <summary>
        /// Unikalny identyfikator budynku (klucz g³ówny).
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Numer budynku (np. 1, 2, 10).
        /// Krotka (Street, Number, Letter) jest unikalna.
        /// </summary>
        [Range(1, 300)]
        public int Number { get; set; } = default!;

        /// <summary>
        /// Litera budynku (np. A, B, C), jeœli wystêpuje.
        /// </summary>
        [MaxLength(3)]
        public string? Letter { get; set; }

        /// <summary>
        /// Liczba piêter w budynku. Wartoœæ -1 oznacza brak danych.
        /// </summary>
        [DefaultValue(-1)]
        public int FloorCount { get; set; }

        /// <summary>
        /// Liczba mieszkañ w budynku. Wartoœæ -1 oznacza brak danych.
        /// </summary>
        [DefaultValue(-1)]
        public int ApartmentCount { get; set; }

        /// <summary>
        /// Najwy¿szy numer mieszkania w budynku. Wartoœæ -1 oznacza brak danych.
        /// </summary>
        [DefaultValue(-1)]
        public int HighestApartmentNumber { get; set; }

        /// <summary>
        /// Okreœla, czy budynek posiada windê. Jest to u¿ywane do planowania wizyt w budynku.
        /// Wizyty w budynkach bez windy s¹ planowane od parteru w górê, podczas gdy 
        /// w budynkach z wind¹ planowane s¹ od najwy¿szego piêtra w dó³.
        /// </summary>
        [DefaultValue(false)]
        public bool HasElevator { get; set; }

        /// <summary>
        /// Okreœla, czy budynek jest dostêpny do wyboru w formularzu internetowym dla niezalogowanego u¿ytkownika.
        /// </summary>
        [DefaultValue(true)]
        public bool AllowSelection { get; set; }

        /// <summary>
        /// Identyfikator ulicy, przy której znajduje siê budynek.
        /// </summary>
        public int StreetId { get; set; }

        /// <summary>
        /// Ulica, przy której znajduje siê budynek (relacja nawigacyjna).
        /// </summary>
        public Street Street { get; set; } = default!;

        /// <summary>
        /// Lista przypisañ agend powi¹zanych z budynkiem. To klasa pomocnicza relacji wiele-do-wielu miêdzy agend¹ a budynkami.
        /// </summary>
        public ICollection<BuildingAssignment> BuildingAssignments { get; set; } = new List<BuildingAssignment>();

        /// <summary>
        /// Lista agend, do których budynek jest przypisany. Ka¿dy budynek mo¿e byæ przypisany do wielu agend.
        /// </summary>
        public ICollection<Agenda> Agendas { get; set; } = new List<Agenda>();

        /// <summary>
        /// Lista jednostek adresowych w budynku.
        /// </summary>
        public ICollection<Address> Addresses { get; set; } = new List<Address>();
    }

    public class BuildingEntityTypeConfiguration : IEntityTypeConfiguration<Building>
    {
        public void Configure(EntityTypeBuilder<Building> builder)
        {
            // Klucz g³ówny
            // (zdefiniowany przez atrybut [Key] w modelu)

            // Indeksy i unikalnoœæ
            builder.HasIndex(b => new { b.StreetId, b.Number, b.Letter })
                   .IsUnique();

            // Generowane pola
            // (brak automatycznie generowanych pól)

            // Relacje
            builder.HasOne(b => b.Street)
                .WithMany(s => s.Buildings)
                .HasForeignKey(b => b.StreetId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(b => b.Agendas)
                .WithMany(a => a.BuildingsAssigned)
                .UsingEntity<BuildingAssignment>();
        }
    }
}