using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace app.Models.Parish
{
    /// <summary>
    /// Reprezentuje plan wizyt duszpasterskich dla danej parafii.
    /// Plan zawiera informacje o autorze, parafii, harmonogramach, zg³oszeniach oraz dniach wizyt.
    /// </summary>
    public class Plan
    {
        /// <summary>
        /// Unikalny identyfikator planu (klucz g³ówny).
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Data i godzina utworzenia planu.
        /// </summary>
        public DateTime CreationTime { get; private set; }

        /// <summary>
        /// Identyfikator u¿ytkownika, który jest autorem planu.
        /// </summary>
        public int AuthorId { get; set; }

        /// <summary>
        /// Autor planu (relacja nawigacyjna).
        /// </summary>
        public User Author { get; set; } = default!;

        /// <summary>
        /// Lista harmonogramów (Schedule) powi¹zanych z planem.
        /// </summary>
        public ICollection<Schedule> Schedules { get; set; } = new List<Schedule>();

        /// <summary>
        /// Lista zg³oszeñ (Submission) powi¹zanych z planem.
        /// </summary>
        public ICollection<Submission> Submissions { get; set; } = new List<Submission>();

        /// <summary>
        /// Lista dni (Day) w ramach planu.
        /// </summary>
        public ICollection<Day> Days { get; set; } = new List<Day>();
    }

    public class PlanEntityTypeConfiguration : IEntityTypeConfiguration<Plan>
    {
        public void Configure(EntityTypeBuilder<Plan> builder)
        {
            // Klucz g³ówny
            // (zdefiniowany przez atrybut [Key] w modelu)

            // Indeksy i unikalnoœæ
            // (nie ma potrzeby dodatkowych indeksów poza kluczem g³ównym)

            // Generowane pola
            builder.Property(fs => fs.CreationTime)
                .HasDefaultValueSql("GETUTCDATE()")
                .ValueGeneratedOnAdd()
                .Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);

            // Relacje
            builder.HasOne(p => p.Author)
                .WithMany()
                .HasForeignKey(p => p.AuthorId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}