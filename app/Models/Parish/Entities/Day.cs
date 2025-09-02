using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace app.Models.Parish.Entities
{
    /// <summary>
    /// Reprezentuje pojedynczy dzieñ w planie wizyt.
    /// Okreœla datê oraz domyœlne godziny rozpoczêcia i zakoñczenia wizyt.
    /// </summary>
    public class Day
    {
        /// <summary>
        /// Unikalny identyfikator dnia (klucz g³ówny).
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Data, której dotyczy dzieñ.
        /// </summary>
        public DateOnly Date { get; set; }

        /// <summary>
        /// Domyœlna godzina rozpoczêcia wizyt w tym dniu. Mo¿e byæ nadpisywana przez agendy.
        /// </summary>
        public TimeOnly StartHour { get; set; }

        /// <summary>
        /// Domyœlna godzina zakoñczenia wizyt w tym dniu. Mo¿e byæ nadpisywana przez agendy.
        /// </summary>
        public TimeOnly EndHour { get; set; }

        /// <summary>
        /// Identyfikator planu, do którego nale¿y ten dzieñ.
        /// </summary>
        public int PlanId { get; set; }

        /// <summary>
        /// Plan, do którego nale¿y ten dzieñ (relacja nawigacyjna).
        /// </summary>
        public Plan Plan { get; set; } = default!;

        /// <summary>
        /// Lista agend przypisanych do tego dnia.
        /// </summary>
        public ICollection<Agenda> Agendas { get; set; } = new List<Agenda>();
    }

    public class DayEntityTypeConfiguration : IEntityTypeConfiguration<Day>
    {
        public void Configure(EntityTypeBuilder<Day> builder)
        {
            // Klucz g³ówny
            // (zdefiniowany przez atrybut [Key] w modelu)

            // Indeksy i unikalnoœæ
            // (nie ma potrzeby dodatkowych indeksów poza kluczem g³ównym)

            // Generowane pola
            // (brak automatycznie generowanych pól)

            // Relacje
            builder.HasOne(d => d.Plan)
                .WithMany(p => p.Days)
                .HasForeignKey(d => d.PlanId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}