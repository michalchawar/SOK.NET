using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SOK.Application.Common.Interface;
using SOK.Infrastructure.Persistence.Context;
using SOK.Infrastructure.Persistence.Seeding;
using SOK.Infrastructure.Repositories;

namespace SOK.Infrastructure.Extensions
{
    /// <summary>
    /// Klasa do rozszerzania funkcjonalności związanych z repozytoriami.
    /// </summary>
    public static class RepositoryExtensions
    {
        /// <summary>
        /// Rejestruje wszystkie potrzebne repozytoria.
        /// </summary>
        /// <param name="configuration">Obiekt, reprezentujący konfigurację aplikacji.</param>
        /// <returns>
        /// Obiekt <see cref="IServiceCollection"/> do tworzenia łańcuchu wywołań.
        /// </returns>
        public static IServiceCollection RegisterRepositories(this IServiceCollection services)
        {
            // Rejestracja UnitOfWork
            services.AddScoped<IUnitOfWorkCentral, UnitOfWorkCentral>();
            services.AddScoped<IUnitOfWorkParish, UnitOfWorkParish>();

            // Rejestracja repozytoriów centralnych
            services.AddScoped<IParishRepository, ParishRepository>();

            // Rejestracja repozytoriów parafialnych
            services.AddScoped<IParishInfoRepository, ParishInfoRepository>();

            return services;
        }
    }
}
