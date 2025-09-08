using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SOK.Domain.Entities.Parish;

namespace SOK.Infrastructure.Persistence.Configurations.Parish
{
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