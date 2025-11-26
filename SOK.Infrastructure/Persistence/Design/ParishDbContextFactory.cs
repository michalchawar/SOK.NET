using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using SOK.Application.Common.DTO;
using SOK.Application.Services.Interface;
using SOK.Domain.Entities.Central;
using SOK.Infrastructure.Persistence.Context;

namespace SOK.Infrastructure.Persistence.Design
{
    public class ParishDbContextFactory : IDesignTimeDbContextFactory<ParishDbContext>
    {
        public ParishDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ParishDbContext>();

            // fallback do LocalDB (żeby migracje działały lokalnie)
            var connectionString = "Server=(localdb)\\MSSQLLocalDB;Database=ParishDbDummy;Trusted_Connection=True;";

            optionsBuilder.UseSqlServer(connectionString);

            // podajemy stub serwisu zamiast prawdziwego
            var fakeService = new DesignTimeParishService
            {
                ConnectionString = connectionString
            };

            return new ParishDbContext(optionsBuilder.Options, fakeService);
        }
    }

    // Prosty stub serwisu ICurrentParishService do użycia w czasie projektowania (migracje, narzędzia EF Core)
    internal class DesignTimeParishService : ICurrentParishService
    {
        public string? ConnectionString { get; set; } = string.Empty;
        public string? ParishUid { get; set; } = string.Empty;

        public Task<bool> SetParishAsync(string parishUid) => Task.FromResult(false);

        public Task<ParishDto> BindParishAsync() => Task.FromResult(new ParishDto());

        public Task<ParishEntry?> GetCurrentParishAsync() => Task.FromResult(new ParishEntry())!;

        public bool IsParishSet() => false;
    }
}
