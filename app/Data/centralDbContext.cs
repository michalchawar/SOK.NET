using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using app.Models.Central;

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
            
        }
    }
}
