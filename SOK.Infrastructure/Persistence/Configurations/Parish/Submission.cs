using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SOK.Domain.Entities.Parish;

namespace SOK.Infrastructure.Persistence.Configurations.Parish
{
    public class SubmissionEntityTypeConfiguration : IEntityTypeConfiguration<Submission>
    {
        public void Configure(EntityTypeBuilder<Submission> builder)
        {
            // Klucz główny
            // (zdefiniowany przez atrybut [Key] w modelu)

            // Indeksy i unikalność
            builder.HasIndex(s => s.UniqueId)
                .IsUnique();

            // Generowane pola
            builder.Property(s => s.AccessToken)
                .HasDefaultValueSql("CONVERT(NVARCHAR(64), HASHBYTES('SHA2_256', CAST(NEWID() AS NVARCHAR(36))), 2)")
                .ValueGeneratedOnAdd();

            builder.Property(s => s.SubmitTime)
                .HasDefaultValueSql("GETUTCDATE()")
                .ValueGeneratedOnAdd()
                .Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);

            // Relacje
            builder.HasOne(s => s.Submitter)
                .WithMany(s => s.Submissions)
                .HasForeignKey(s => s.SubmitterId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(s => s.Address)
                .WithOne(a => a.Submission)
                .HasForeignKey<Submission>(s => s.AddressId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(s => s.Plan)
                .WithMany(p => p.Submissions)
                .HasForeignKey(s => s.PlanId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}