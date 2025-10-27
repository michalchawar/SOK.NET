using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SOK.Domain.Entities.Parish;

namespace SOK.Infrastructure.Persistence.Configurations.Parish
{
    public class ParishMemberEntityTypeConfiguration : IEntityTypeConfiguration<ParishMember>
    {
        public void Configure(EntityTypeBuilder<ParishMember> builder)
        {
            // Klucz g³ówny
            // (zdefiniowany przez atrybut [Key] w modelu)

            // Indeksy i unikalnoœæ
            builder.HasIndex(u => u.CentralUserId)
                .IsUnique();

            // Generowane pola
            // (brak automatycznie generowanych pól)

            // Relacje
            // (ParishMember nie jest podrzêdny wzglêdem ¿adnej encji, nie konfigurujemy relacji)
        }
    }
}