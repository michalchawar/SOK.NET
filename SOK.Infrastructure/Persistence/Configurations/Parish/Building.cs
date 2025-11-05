using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SOK.Domain.Entities.Parish;

namespace SOK.Infrastructure.Persistence.Configurations.Parish
{
    public class BuildingEntityTypeConfiguration : IEntityTypeConfiguration<Building>
    {
        public void Configure(EntityTypeBuilder<Building> builder)
        {
            // Klucz g��wny
            // (zdefiniowany przez atrybut [Key] w modelu)

            // Indeksy i unikalno��
            builder.HasIndex(b => new { b.StreetId, b.Number, b.Letter })
                   .IsUnique();

            // Generowane pola
            // (brak automatycznie generowanych p�l)

            // Relacje
            builder.HasOne(b => b.Street)
                .WithMany(s => s.Buildings)
                .HasForeignKey(b => b.StreetId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(b => b.Days)
                .WithMany(a => a.BuildingsAssigned)
                .UsingEntity<BuildingAssignment>();

            // Wyzwalacze
            builder.ToTable(t => t.HasTrigger("TR_Building_Update_AddressCache"));
        }
    }
}