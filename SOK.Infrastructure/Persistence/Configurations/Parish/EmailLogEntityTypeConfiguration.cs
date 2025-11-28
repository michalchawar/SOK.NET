using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SOK.Domain.Entities.Parish;

namespace SOK.Infrastructure.Persistence.Configurations.Parish
{
    public class EmailLogEntityTypeConfiguration : IEntityTypeConfiguration<EmailLog>
    {
        public void Configure(EntityTypeBuilder<EmailLog> builder)
        {
            // Klucz główny
            // (zdefiniowany przez atrybut [Key] w modelu)

            // Indeksy i unikalność
            // (dla wydajności)
            builder.HasIndex(e => new { e.Sent, e.Priority });
            builder.HasIndex(e => e.SubmissionId);

            // Generowane pola
            builder.Property(e => e.AddedTimestamp)
                .HasDefaultValueSql("GETUTCDATE()")
                .ValueGeneratedOnAdd()
                .Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);

            builder.Property(e => e.SentTimestamp)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            // Relacje
            builder.HasOne(e => e.Submission)
                .WithMany(s => s.EmailLogs)
                .HasForeignKey(e => e.SubmissionId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
