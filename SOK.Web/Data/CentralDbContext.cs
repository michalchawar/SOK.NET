using SOK.Web.Models;
using SOK.Web.Models.Central.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SOK.Web.Data
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

    public class CentralDbContextFactory : IDesignTimeDbContextFactory<CentralDbContext>
    {
        public CentralDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<CentralDbContext>();

            // 1. spróbuj pobrać z env (jak w Dockerze)
            var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__CentralDb");

            // 2. fallback dla migracji odpalanych lokalnie
            if (string.IsNullOrEmpty(connectionString))
            {
                connectionString = "Server=(localdb)\\MSSQLLocalDB;Database=CentralDbFallback;Trusted_Connection=True;";
            }

            optionsBuilder.UseSqlServer(connectionString);

            return new CentralDbContext(optionsBuilder.Options);
        }
    }
}
