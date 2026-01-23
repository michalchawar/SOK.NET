using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using SOK.Application.Common.DTO.Parish;
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
            var fakeParishService = new DesignTimeParishService
            {
                ConnectionString = connectionString
            };

            var fakeHttpContextAccessor = new DesignTimeHttpContextAccessor();
            var fakeLogger = NullLogger<ParishDbContext>.Instance;

            return new ParishDbContext(optionsBuilder.Options, fakeParishService, fakeHttpContextAccessor, fakeLogger);
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

    // Stub IHttpContextAccessor do użycia w czasie projektowania
    internal class DesignTimeHttpContextAccessor : IHttpContextAccessor
    {
        public HttpContext? HttpContext { get; set; } = null;
    }
}
