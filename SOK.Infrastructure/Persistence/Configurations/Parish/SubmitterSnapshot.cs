using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SOK.Domain.Entities.Parish;

namespace SOK.Infrastructure.Persistence.Configurations.Parish
{
    public class SubmitterSnapshotEntityTypeConfiguration : IEntityTypeConfiguration<SubmitterSnapshot>
    {
        public void Configure(EntityTypeBuilder<SubmitterSnapshot> builder)
        {
            // Klucz główny
            // (zdefiniowany przez atrybut [Key] w modelu)

            // Indeksy i unikalność
            // (nie ma potrzeby dodatkowych indeksów poza kluczem głównym)

            // Generowane pola
            builder.Property(ss => ss.ChangeTime)
                .HasDefaultValueSql("GETUTCDATE()")
                .ValueGeneratedOnAdd();

            // Relacje
            builder.HasOne(ss => ss.ChangeAuthor)
                .WithMany()
                .HasForeignKey(ss => ss.ChangeAuthorId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}