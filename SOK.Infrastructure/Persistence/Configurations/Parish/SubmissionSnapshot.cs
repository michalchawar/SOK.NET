using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SOK.Domain.Entities.Parish;

namespace SOK.Infrastructure.Persistence.Configurations.Parish
{
    public class SubmissionSnapshotEntityTypeConfiguration : IEntityTypeConfiguration<SubmissionSnapshot>
    {
        public void Configure(EntityTypeBuilder<SubmissionSnapshot> builder)
        {
            // Klucz g³ówny
            // (zdefiniowany przez atrybut [Key] w modelu)

            // Indeksy i unikalnoœæ
            // (nie ma potrzeby dodatkowych indeksów poza kluczem g³ównym)

            // Generowane pola
            builder.Property(ss => ss.ChangeTime)
                .HasDefaultValueSql("GETUTCDATE()")
                .ValueGeneratedOnAdd()
                .Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);

            // Relacje
            builder.HasOne(ss => ss.Submission)
                .WithMany(s => s.History)
                .HasForeignKey(ss => ss.SubmissionId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(ss => ss.ChangeAuthor)
                .WithMany()
                .HasForeignKey(ss => ss.ChangeAuthorId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}