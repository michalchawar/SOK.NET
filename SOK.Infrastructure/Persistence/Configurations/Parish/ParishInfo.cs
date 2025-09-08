using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SOK.Domain.Entities.Parish;

namespace SOK.Infrastructure.Persistence.Configurations.Parish
{
    public class ParishInfoEntityTypeConfiguration : IEntityTypeConfiguration<ParishInfo>
    {
        public void Configure(EntityTypeBuilder<ParishInfo> builder)
        {
            // Klucz główny
            // (zdefiniowany przez atrybut [Key] w modelu)

            // Indeksy i unikalność
            builder.HasIndex(p => p.Name)
                .IsUnique();

            // Generowane pola
            // (brak automatycznie generowanych pól)

            // Relacje
            // (ParishInfo nie jest podrzędne względem żadnej encji, nie konfigurujemy relacji)
        }
    }
}