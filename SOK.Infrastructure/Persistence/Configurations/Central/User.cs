using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SOK.Domain.Entities.Central;

namespace SOK.Infrastructure.Persistence.Configurations.Central
{
    public class UserEntityTypeConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            // Klucz g³ówny
            // (zdefiniowany przez atrybut [Key] w modelu)

            // Indeksy i unikalnoœæ
            // (nie ma potrzeby dodatkowych indeksów poza kluczem g³ównym)

            // Generowane pola
            // (brak automatycznie generowanych pól)

            // Relacje
            builder.HasOne(u => u.Parish)
                .WithMany(p => p.Users)
                .HasForeignKey(u => u.ParishId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}