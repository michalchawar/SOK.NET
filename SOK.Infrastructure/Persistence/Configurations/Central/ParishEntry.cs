using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SOK.Domain.Entities.Central;

namespace SOK.Infrastructure.Persistence.Configurations.Central
{
    public class ParishEntryEntityTypeConfiguration : IEntityTypeConfiguration<ParishEntry>
    {
        public void Configure(EntityTypeBuilder<ParishEntry> builder)
        {
            // Klucz g³ówny
            // (zdefiniowany przez atrybut [Key] w modelu)

            // Indeksy i unikalnoœæ
            builder.HasIndex(p => p.UniqueId)
                .IsUnique();

            // Generowane pola
            builder.Property(p => p.CreationTime)
                .HasDefaultValueSql("GETUTCDATE()")
                .ValueGeneratedOnAdd()
                .Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);

            // Relacje
            // (ParishEntry nie jest podrzêdny wzglêdem ¿adnej encji, nie konfigurujemy relacji)
        }
    }
}