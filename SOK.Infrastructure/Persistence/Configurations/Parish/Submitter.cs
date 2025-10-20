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
            builder.HasIndex(s => s.FilterableString);

            // Generowane pola
            builder.Property(s => s.FilterableString)
                .HasComputedColumnSql(
                    // ³¹czymy dane w ró¿nych kolejnoœciach i ma³ymi literami
                    "LOWER(CONCAT_WS(' ', " +
                        "COALESCE(Name, ''), " +
                        "COALESCE(Surname, ''), " +
                        "COALESCE(Name, ''), " +
                        "COALESCE(Email, ''), " +
                        "COALESCE(Phone, '')" +
                    "))",
                    stored: true);


            // Relacje
            // (Submitter nie jest podrzêdne wzglêdem ¿adnej encji, nie konfigurujemy relacji)
        }
    }
}