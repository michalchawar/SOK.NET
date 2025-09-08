using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SOK.Domain.Entities.Parish;

namespace SOK.Infrastructure.Persistence.Configurations.Parish
{
    public class StreetSpecifierEntityTypeConfiguration : IEntityTypeConfiguration<StreetSpecifier>
    {
        public void Configure(EntityTypeBuilder<StreetSpecifier> builder)
        {
            // Klucz g³ówny
            // (zdefiniowany przez atrybut [Key] w modelu)

            // Indeksy i unikalnoœæ
            builder.HasIndex(s => s.FullName)
                .IsUnique();

            // Generowane pola
            // (brak automatycznie generowanych pól)

            // Relacje
            // (StreetSpecifier nie jest podrzêdne wzglêdem ¿adnej encji, nie konfigurujemy relacji)
        }
    }
}