using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SOK.Domain.Entities.Parish;

namespace SOK.Infrastructure.Persistence.Configurations.Parish
{
    public class AddressEntityTypeConfiguration : IEntityTypeConfiguration<Address>
    {
        public void Configure(EntityTypeBuilder<Address> builder)
        {
            // Klucz główny
            // (zdefiniowany przez atrybut [Key] w modelu)

            // Indeksy i unikalność
            builder.HasIndex(a => new { a.BuildingId, a.ApartmentNumber, a.ApartmentLetter })
                .IsUnique()
                .HasFilter(null);
            builder.HasIndex(a => a.FilterableString);

            // Generowane pola
            builder.Property(a => a.FilterableString)
                .HasComputedColumnSql(
                    // łączymy dane w różnych kolejnościach i małymi literami
                    "LOWER(CONCAT_WS(' ', " +
                        "COALESCE(StreetType, ''), " +
                        "COALESCE(StreetName, ''), " +
                        "CONCAT(" +
                            "COALESCE(BuildingNumber, ''), " +
                            "COALESCE(BuildingLetter, '')), " +
                        "CONCAT(" +
                            "COALESCE(ApartmentNumber, ''), " +
                            "COALESCE(ApartmentLetter, '')), " +
                        "COALESCE(CityName, '')" +
                    "))",
                    stored: true);

            // Relacje
            builder.HasOne(a => a.Building)
                .WithMany(b => b.Addresses)
                .HasForeignKey(a => a.BuildingId)
                .OnDelete(DeleteBehavior.Cascade);

            // Wyzwalacze
            builder.ToTable(t => t.HasTrigger("TR_Address_InsertOrUpdate_Cache"));
        }
    }
}