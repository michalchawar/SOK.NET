using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using SOK.Infrastructure.Persistence.Context;

namespace SOK.Infrastructure.Persistence.Design
{
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
