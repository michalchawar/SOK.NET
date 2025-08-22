using app.Data;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace app.Extensions
{
    /// <summary>
    /// Klasa do rozszerzania funkcjonalności związanych z bazą danych.
    /// </summary>
    public static class DatabaseExtensions
    {
        /// <summary>
        /// Sprawdza i wykonuje migracje centralnej bazy danych.
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
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

        public static IServiceCollection RegisterDatabaseContexts(this IServiceCollection services, IConfiguration configuration)
        {
            // Zarejestruj kontekst centralnej bazy danych
            services.AddDbContext<CentralDbContext>(options =>
            {
                options.UseSqlServer(configuration["ConnectionStrings:CentralDb"]
                    ?? throw new InvalidOperationException("Connection string for central database (CentralDb) not found."));
            });

            // Zarejestruj kontekst indywidualnej bazy danych parafii (tenant'a)
            services.AddDbContext<ParishDbContext>((options) => options.UseSqlServer(""));

            return services;
        }
    }
}
