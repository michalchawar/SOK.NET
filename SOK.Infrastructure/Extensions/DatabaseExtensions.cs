using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SOK.Application.Common.Interface;
using SOK.Infrastructure.Persistence.Context;
using SOK.Infrastructure.Persistence.Seeding;

namespace SOK.Infrastructure.Extensions
{
    /// <summary>
    /// Klasa do rozszerzania funkcjonalności związanych z bazą danych.
    /// </summary>
    public static class DatabaseExtensions
    {
        /// <summary>
        /// Sprawdza i wykonuje migracje centralnej bazy danych.
        /// </summary>
        /// <returns>
        /// Obiekt <see cref="IServiceCollection"/> do tworzenia łańcuchu wywołań.
        /// </returns>
        public static IServiceCollection MigrateCentralDatabase(this IServiceCollection services)
        {
            // Pobierz kontekst bazy danych
            using IServiceScope serviceScope = services.BuildServiceProvider().CreateScope();
            CentralDbContext context = serviceScope.ServiceProvider.GetRequiredService<CentralDbContext>();

            // Sprawdź i wykonaj migracje, jeśli oczekują
            if (context.Database.GetPendingMigrations().Any())
            {
                Console.ForegroundColor = ConsoleColor.DarkCyan;
                Console.WriteLine("Applying migrations for the central database...");
                Console.ResetColor();
                context.Database.Migrate();
            }

            return services;
        }

        /// <summary>
        /// Rejestruje potrzebne konteksty baz danych.
        /// </summary>
        /// <param name="configuration">Obiekt, reprezentujący konfigurację aplikacji.</param>
        /// <returns>
        /// Obiekt <see cref="IServiceCollection"/> do tworzenia łańcuchu wywołań.
        /// </returns>
        /// <exception cref="InvalidOperationException"></exception>
        /// <remarks>
        /// Rejestruje u dostawcy usług dwa konteksty baz danych: kontekst centralnej
        /// bazy danych (<see cref="CentralDbContext"/>) oraz kontekst indywidualnej bazy danych
        /// (<see cref="ParishDbContext"/>). Połączenie do centralnej bazy danych używa
        /// <c>ConnectionString</c> z konfiguracji aplikacji, natomiast
        /// połączenie do indywidualnej bazy danych jest ustawione na pusty łańcuch znaków,
        /// później będzie dynamicznie konfigurowane w zależności od parafii (tenant'a)
        /// w nadpisaniu metody <see cref="ParishDbContext.OnConfiguring(DbContextOptionsBuilder)"/>.
        /// </remarks>
        public static IServiceCollection RegisterDatabaseContexts(this IServiceCollection services, IConfiguration configuration)
        {
            // Zarejestruj kontekst centralnej bazy danych
            services.AddDbContext<CentralDbContext>(options =>
            {
                options.UseSqlServer(
                    configuration["ConnectionStrings:CentralDb"]
                        ?? throw new InvalidOperationException("Connection string for central database (CentralDb) not found."),
                    sqlServerOptions => sqlServerOptions.EnableRetryOnFailure());
            });

            // Zarejestruj kontekst indywidualnej bazy danych parafii (tenant'a)
            services.AddDbContext<ParishDbContext>();

            // Zarejestruj seedery baz danych
            services.AddScoped<IDbSeeder, DbSeeder>();

            return services;
        }
    }
}
