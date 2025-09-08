using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SOK.Domain.Entities.Parish;

namespace SOK.Infrastructure.Persistence.Configurations.Parish
{
    public class ScheduleEntityTypeConfiguration : IEntityTypeConfiguration<Schedule>
    {
        public void Configure(EntityTypeBuilder<Schedule> builder)
        {
            // Klucz g³ówny
            // (zdefiniowany przez atrybut [Key] w modelu)

            // Indeksy i unikalnoœæ
            builder.HasIndex(s => s.Name)
                .IsUnique();

            builder.HasIndex(s => s.ShortName)
                .IsUnique();

            // Generowane pola
            // (brak automatycznie generowanych pól)

            // Relacje
            builder.HasOne(s => s.Plan)
                .WithMany(p => p.Schedules)
                .HasForeignKey(s => s.PlanId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}