using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SOK.Domain.Entities.Parish;

namespace SOK.Infrastructure.Persistence.Configurations.Parish
{
    public class UserEntityTypeConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            // Klucz g³ówny
            // (zdefiniowany przez atrybut [Key] w modelu)

            // Indeksy i unikalnoœæ
            builder.HasIndex(u => u.Username)
                .IsUnique();

            // Generowane pola
            // (brak automatycznie generowanych pól)

            // Relacje
            // (User nie jest podrzêdny wzglêdem ¿adnej encji, nie konfigurujemy relacji)
        }
    }
}