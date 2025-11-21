using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SOK.Domain.Entities.Parish;

namespace SOK.Infrastructure.Persistence.Configurations.Parish
{
    public class CityEntityTypeConfiguration : IEntityTypeConfiguration<City>
    {
        public void Configure(EntityTypeBuilder<City> builder)
        {
            // Klucz główny
            // (zdefiniowany przez atrybut [Key] w modelu)

            // Indeksy i unikalność
            // (nie ma potrzeby dodatkowych indeksów poza kluczem głównym)

            // Generowane pola
            // (brak automatycznie generowanych pól)

            // Relacje
            // (City nie jest podrzędne względem żadnej encji, nie konfigurujemy relacji)

            // Wyzwalacze
            builder.ToTable(t => t.HasTrigger("TR_City_Update_AddressCache"));
        }
    }
}