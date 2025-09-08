using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SOK.Domain.Entities.Parish;

namespace SOK.Infrastructure.Persistence.Configurations.Parish
{
    public class BuildingAssignmentEntityTypeConfiguration : IEntityTypeConfiguration<BuildingAssignment>
    {
        public void Configure(EntityTypeBuilder<BuildingAssignment> builder)
        {
            // Klucz g³ówny
            builder.HasKey(ba => new { ba.AgendaId, ba.BuildingId, ba.ScheduleId });

            // Indeksy i unikalnoœæ
            // (nie ma potrzeby dodatkowych indeksów poza kluczem g³ównym)

            // Generowane pola
            // (brak automatycznie generowanych pól)

            // Relacje
            builder.HasOne(ba => ba.Agenda)
                .WithMany(a => a.BuildingAssignments)
                .HasForeignKey(ba => ba.AgendaId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(ba => ba.Building)
                .WithMany(b => b.BuildingAssignments)
                .HasForeignKey(ba => ba.BuildingId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(ba => ba.Schedule)
                .WithMany(s => s.BuildingAssignments)
                .HasForeignKey(ba => ba.ScheduleId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}