using app.Models;
using app.Models.Central;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace app.Data
{
    public class centralDbContext : DbContext
    {
        public centralDbContext(DbContextOptions<centralDbContext> options)
            : base(options)
        {
        }

        public DbSet<app.Models.Central.User> Users { get; set; } = default!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Rejestracja konfiguracji encji
            modelBuilder.ApplyConfiguration(new app.Models.Central.UserEntityTypeConfiguration());
        }
    }
}
