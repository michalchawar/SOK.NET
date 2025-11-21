using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SOK.Domain.Entities.Parish;

namespace SOK.Infrastructure.Persistence.Configurations.Parish
{
    public class AgendaEntityTypeConfiguration : IEntityTypeConfiguration<Agenda>
    {
        public void Configure(EntityTypeBuilder<Agenda> builder)
        {
            // Klucz główny
            // (zdefiniowany przez atrybut [Key] w modelu)

            // Indeksy i unikalność
            builder.HasIndex(a => a.UniqueId)
                .IsUnique();

            // Generowane pola
            builder.Property(a => a.AccessToken)
                .HasDefaultValueSql("CONVERT(varchar(64), HASHBYTES('SHA2_256', CAST(NEWID() AS varchar(36))), 2)");

            // Relacje
            builder.HasOne(a => a.Day)
                .WithMany(d => d.Agendas)
                .HasForeignKey(a => a.DayId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(a => a.AssignedMembers)
                .WithMany(u => u.AssignedAgendas);
        }
    }
}