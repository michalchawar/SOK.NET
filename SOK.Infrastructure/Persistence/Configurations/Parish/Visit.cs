using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SOK.Domain.Entities.Parish;

namespace SOK.Infrastructure.Persistence.Configurations.Parish
{
    public class VisitEntityTypeConfiguration : IEntityTypeConfiguration<Visit>
    {
        public void Configure(EntityTypeBuilder<Visit> builder)
        {
            // Klucz główny
            // (zdefiniowany przez atrybut [Key] w modelu)

            // Indeksy i unikalność
            builder.HasIndex(v => new { v.AgendaId, v.OrdinalNumber })
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