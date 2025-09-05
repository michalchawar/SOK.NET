using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SOK.Domain.Entities.Central;
using SOK.Infrastructure.Persistence.Configurations.Central;

namespace SOK.Infrastructure.Persistence.Context
{
    public class CentralDbContext : IdentityDbContext<User>
    {
        public CentralDbContext(DbContextOptions<CentralDbContext> options)
            : base(options)
        {
        }

        public DbSet<ParishEntry> Parishes { get; set; } = default!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Rejestracja konfiguracji encji
            modelBuilder.ApplyConfiguration(new UserEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new ParishEntryEntityTypeConfiguration());

            base.OnModelCreating(modelBuilder);
        }
    }
}
