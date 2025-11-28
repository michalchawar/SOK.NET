using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SOK.Domain.Entities.Parish;

namespace SOK.Infrastructure.Persistence.Configurations.Parish
{
    public class VisitSnapshotEntityTypeConfiguration : IEntityTypeConfiguration<VisitSnapshot>
    {
        public void Configure(EntityTypeBuilder<VisitSnapshot> builder)
        {
            // Klucz główny
            // (zdefiniowany przez atrybut [Key] w modelu)

            // Indeksy i unikalność
            // (nie ma potrzeby dodatkowych indeksów poza kluczem głównym)

            // Generowane pola
            builder.Property(vs => vs.ChangeTime)
                .HasDefaultValueSql("GETUTCDATE()")
                .ValueGeneratedOnAdd()
                .Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);

            // Relacje
            builder.HasOne(vs => vs.Visit)
                .WithMany(v => v.History)
                .HasForeignKey(vs => vs.VisitId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(vs => vs.ChangeAuthor)
                .WithMany()
                .HasForeignKey(vs => vs.ChangeAuthorId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}