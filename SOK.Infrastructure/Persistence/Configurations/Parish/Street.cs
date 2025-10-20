using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SOK.Domain.Entities.Parish;

namespace SOK.Infrastructure.Persistence.Configurations.Parish
{
    public class StreetEntityTypeConfiguration : IEntityTypeConfiguration<Street>
    {
        public void Configure(EntityTypeBuilder<Street> builder)
        {
            // Klucz g³ówny
            // (zdefiniowany przez atrybut [Key] w modelu)

            // Indeksy i unikalnoœæ
            builder.HasIndex(s => s.Name)
                .IsUnique();

            // Generowane pola
            // (brak automatycznie generowanych pól)

            // Relacje
            builder.HasOne(s => s.Type)
                .WithMany()
                .HasForeignKey(s => s.StreetSpecifierId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(s => s.City)
                .WithMany(c => c.Streets)
                .HasForeignKey(s => s.CityId)
                .OnDelete(DeleteBehavior.Cascade);

            // Wyzwalacze
            builder.ToTable(t => t.HasTrigger("TR_Street_Update_AddressCache"));
        }
    }
}