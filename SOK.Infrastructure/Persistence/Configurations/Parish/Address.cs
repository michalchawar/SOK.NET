using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SOK.Domain.Entities.Parish;

namespace SOK.Infrastructure.Persistence.Configurations.Parish
{
    public class AddressEntityTypeConfiguration : IEntityTypeConfiguration<Address>
    {
        public void Configure(EntityTypeBuilder<Address> builder)
        {
            // Klucz g³ówny
            // (zdefiniowany przez atrybut [Key] w modelu)

            // Indeksy i unikalnoœæ
            builder.HasIndex(a => new { a.BuildingId, a.ApartmentNumber, a.ApartmentLetter })
                .IsUnique();

            // Generowane pola
            // (brak automatycznie generowanych pól)

            // Relacje
            builder.HasOne(a => a.Building)
                .WithMany(b => b.Addresses)
                .HasForeignKey(a => a.BuildingId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(a => a.Street)
                .WithMany()
                .HasForeignKey(a => a.StreetId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(a => a.City)
                .WithMany()
                .HasForeignKey(a => a.CityId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}