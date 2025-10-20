using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SOK.Domain.Entities.Parish;

namespace SOK.Infrastructure.Persistence.Configurations.Parish
{
    public class SubmitterEntityTypeConfiguration : IEntityTypeConfiguration<Submitter>
    {
        public void Configure(EntityTypeBuilder<Submitter> builder)
        {
            // Klucz g³ówny
            // (zdefiniowany przez atrybut [Key] w modelu)

            // Indeksy i unikalnoœæ
            builder.HasIndex(s => s.UniqueId)
                .IsUnique();

            // Generowane pola
            // (brak automatycznie generowanych pól)

            // Relacje
            // (Submitter nie jest podrzêdne wzglêdem ¿adnej encji, nie konfigurujemy relacji)
        }
    }
}