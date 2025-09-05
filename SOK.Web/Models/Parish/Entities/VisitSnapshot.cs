using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata;
using SOK.Web.Models.Parish.Enums;

namespace SOK.Web.Models.Parish.Entities
{
    /// <summary>
    /// Reprezentuje archiwalny stan wizyty w danym momencie.
    /// Pozwala œledziæ historiê zmian statusu, daty, przewidywanego czasu oraz autora zmiany.
    /// Obiekt wizyty jest przypisany na sta³e do jednego zg³oszenia,
    /// zatem snapshot nie uwzglêdnia zmian zg³oszenia.
    /// </summary>
    public class VisitSnapshot
    {
        /// <summary>
        /// Unikalny identyfikator snapshotu wizyty (klucz g³ówny).
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Numer porz¹dkowy wizyty w ramach agendy w momencie utworzenia snapshotu.
        /// </summary>
        [Range(1, 300)]
        public short OrdinalNumber { get; set; }

        /// <summary>
        /// Status wizyty w momencie utworzenia snapshotu.
        /// </summary>
        [DefaultValue(VisitStatus.Unplanned)]
        public VisitStatus Status { get; set; }

        /// <summary>
        /// Nazwa harmonogramu, do którego przypisana by³a wizyta w momencie utworzenia snapshotu.
        /// Mo¿e byæ null, tylko gdy Status jest równy VisitStatus.Withdrawn.
        /// </summary>
        public string? ScheduleName { get; set; } = default!;

        /// <summary>
        /// Data wizyty w momencie utworzenia snapshotu (jeœli dotyczy).
        /// </summary>
        public DateTime? Date { get; set; }

        /// <summary>
        /// Czy data wizyty by³a widoczna dla u¿ytkownika w momencie utworzenia snapshotu.
        /// </summary>
        public bool? DateVisibility { get; set; }
        
        /// <summary>
        /// Przewidywany czas wizyty w momencie utworzenia snapshotu (jeœli dotyczy).
        /// </summary>
        public TimeOnly? PredictedTime { get; set; }

        /// <summary>
        /// Czy przewidywany czas wizyty by³ widoczny dla u¿ytkownika w momencie utworzenia snapshotu.
        /// </summary>
        public bool? PredictedTimeVisibility { get; set; }

        /// <summary>
        /// Data i godzina utworzenia snapshotu.
        /// </summary>
        public DateTime ChangeTime { get; private set; }

        /// <summary>
        /// Identyfikator u¿ytkownika, który wprowadzi³ zmianê, nadpisuj¹c dane z tego snapshotu.
        /// </summary>
        public int? ChangeAuthorId { get; set; } = default!;

        /// <summary>
        /// U¿ytkownik, który wprowadzi³ zmianê, nadpisuj¹c dane z tego snapshotu (relacja nawigacyjna).
        /// </summary>
        public User? ChangeAuthor { get; set; } = default!;

        /// <summary>
        /// Identyfikator wizyty, której dotyczy snapshot.
        /// </summary>
        public int VisitId { get; set; }

        /// <summary>
        /// Wizyta, której dotyczy snapshot (relacja nawigacyjna).
        /// </summary>
        public Visit Visit { get; set; } = default!;
    }

    public class VisitSnapshotEntityTypeConfiguration : IEntityTypeConfiguration<VisitSnapshot>
    {
        public void Configure(EntityTypeBuilder<VisitSnapshot> builder)
        {
            // Klucz g³ówny
            // (zdefiniowany przez atrybut [Key] w modelu)

            // Indeksy i unikalnoœæ
            // (nie ma potrzeby dodatkowych indeksów poza kluczem g³ównym)

            // Generowane pola
            builder.Property(vs => vs.ChangeTime)
                .HasDefaultValueSql("GETUTCDATE()")
                .ValueGeneratedOnAdd()
                .Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);

            // Relacje
            builder.HasOne(vs => vs.Visit)
                .WithMany(v => v.History)
                .HasForeignKey(vs => vs.VisitId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(vs => vs.ChangeAuthor)
                .WithMany()
                .HasForeignKey(vs => vs.ChangeAuthorId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}