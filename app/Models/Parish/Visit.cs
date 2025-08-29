using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using app.Models.Parish.Enums;

namespace app.Models.Parish
{
    /// <summary>
    /// Reprezentuje pojedyncz¹ wizytê duszpastersk¹ w ramach agendy i harmonogramu.
    /// Zawiera informacje o statusie, kolejnoœci, powi¹zaniach z agend¹, harmonogramem oraz historiê zmian.
    /// </summary>
    public class Visit
    {
        /// <summary>
        /// Unikalny identyfikator wizyty (klucz g³ówny).
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Numer porz¹dkowy wizyty w ramach agendy (planowana kolejnoœæ odwiedzin).
        /// </summary>
        [Range(1, 300)]
        public int? OrdinalNumber { get; set; }

        /// <summary>
        /// Status wizyty. Wizyta nieprzypisana do ¿adnej agendy ma status Unplanned.
        /// </summary>
        [DefaultValue(VisitStatus.Unplanned)]
        public VisitStatus Status { get; set; }

        /// <summary>
        /// Identyfikator agendy, do której przypisana jest wizyta (opcjonalny).
        /// </summary>
        public int? AgendaId { get; set; } = default!;

        /// <summary>
        /// Agenda, do której przypisana jest wizyta (relacja opcjonalna).
        /// </summary>
        public Agenda? Agenda { get; set; } = default!;

        /// <summary>
        /// Identyfikator harmonogramu, do którego nale¿y wizyta (opcjonalny).
        /// Mo¿e byæ null, tylko gdy Status jest równy VisitStatus.Withdrawn.
        /// </summary>
        public int? ScheduleId { get; set; }

        /// <summary>
        /// Harmonogram, do którego nale¿y wizyta (relacja nawigacyjna).
        /// Mo¿e byæ null, tylko gdy Status jest równy VisitStatus.Withdrawn.
        /// </summary>
        public Schedule? Schedule { get; set; } = default!;

        /// <summary>
        /// Identyfikator zg³oszenia, do którego nale¿y wizyta.
        /// </summary>
        public int SubmissionId { get; set; }

        /// <summary>
        /// Zg³oszenie, do którego nale¿y wizyta (relacja nawigacyjna).
        /// </summary>
        public Submission Submission { get; set; } = default!;

        /// <summary>
        /// Historia zmian wizyty (snapshoty).
        /// </summary>
        public ICollection<VisitSnapshot> History { get; set; } = new List<VisitSnapshot>();
    }

    public class VisitEntityTypeConfiguration : IEntityTypeConfiguration<Visit>
    {
        public void Configure(EntityTypeBuilder<Visit> builder)
        {
            // Klucz g³ówny
            // (zdefiniowany przez atrybut [Key] w modelu)

            // Indeksy i unikalnoœæ
            builder.HasIndex(v => new { v.ScheduleId, v.OrdinalNumber })
                .IsUnique();

            // Generowane pola
            // (brak automatycznie generowanych pól)

            // Relacje
            builder.HasOne(v => v.Agenda)
                .WithMany(a => a.Visits)
                .HasForeignKey(v => v.AgendaId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasOne(v => v.Schedule)
                .WithMany(s => s.Visits)
                .HasForeignKey(v => v.ScheduleId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(v => v.Submission)
                .WithOne(s => s.Visit)
                .HasForeignKey<Visit>(v => v.SubmissionId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}