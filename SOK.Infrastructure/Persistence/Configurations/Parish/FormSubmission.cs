using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SOK.Domain.Entities.Parish;

namespace SOK.Infrastructure.Persistence.Configurations.Parish
{
    public class FormSubmissionEntityTypeConfiguration : IEntityTypeConfiguration<FormSubmission>
    {
        public void Configure(EntityTypeBuilder<FormSubmission> builder)
        {
            // Klucz g³ówny
            // (zdefiniowany przez atrybut [Key] w modelu)

            // Indeksy i unikalnoœæ
            // (nie ma potrzeby dodatkowych indeksów poza kluczem g³ównym)

            // Generowane pola
            builder.Property(fs => fs.SubmitTime)
                .HasDefaultValueSql("GETUTCDATE()")
                .ValueGeneratedOnAdd()
                .Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);

            // Relacje
            builder.HasOne(fs => fs.Author)
                .WithMany(u => u.EnteredSubmissions)
                .HasForeignKey(fs => fs.AuthorId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasOne(fs => fs.Submission)
                .WithOne(s => s.FormSubmission)
                .HasForeignKey<FormSubmission>(fs => fs.SubmissionId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}