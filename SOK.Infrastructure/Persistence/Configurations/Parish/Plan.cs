using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SOK.Domain.Entities.Parish;

namespace SOK.Infrastructure.Persistence.Configurations.Parish
{
    public class PlanEntityTypeConfiguration : IEntityTypeConfiguration<Plan>
    {
        public void Configure(EntityTypeBuilder<Plan> builder)
        {
            // Klucz g��wny
            // (zdefiniowany przez atrybut [Key] w modelu)

            // Indeksy i unikalno��
            // (nie ma potrzeby dodatkowych indeks�w poza kluczem g��wnym)

            // Generowane pola
            builder.Property(fs => fs.CreationTime)
                .HasDefaultValueSql("GETUTCDATE()")
                .ValueGeneratedOnAdd()
                .Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);

            // Relacje
            builder.HasOne(p => p.Author)
                .WithMany()
                .HasForeignKey(p => p.AuthorId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasOne(p => p.DefaultSchedule)
                .WithOne()
                .HasForeignKey<Plan>(p => p.DefaultScheduleId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasMany(p => p.ActivePriests)
                .WithMany(pm => pm.AssignedPlans);
        }
    }
}