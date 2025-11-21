using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SOK.Domain.Entities.Parish;

namespace SOK.Infrastructure.Persistence.Configurations.Parish
{
    public class SubmitterEntityTypeConfiguration : IEntityTypeConfiguration<Submitter>
    {
        public void Configure(EntityTypeBuilder<Submitter> builder)
        {
            // Klucz główny
            // (zdefiniowany przez atrybut [Key] w modelu)

            // Indeksy i unikalność
            builder.HasIndex(s => s.UniqueId)
                .IsUnique();
            builder.HasIndex(s => s.FilterableString);

            // Generowane pola
            builder.Property(s => s.FilterableString)
                .HasComputedColumnSql(
                    // łączymy dane w różnych kolejnościach i małymi literami
                    "LOWER(CONCAT_WS(' ', " +
                        "COALESCE(Name, ''), " +
                        "COALESCE(Surname, ''), " +
                        "COALESCE(Name, ''), " +
                        "COALESCE(Email, ''), " +
                        "COALESCE(Phone, '')" +
                    "))",
                    stored: true);


            // Relacje
            // (Submitter nie jest podrzędne względem żadnej encji, nie konfigurujemy relacji)
        }
    }
}