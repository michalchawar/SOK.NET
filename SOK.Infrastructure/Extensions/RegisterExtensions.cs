using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SOK.Application.Common.Interface;
using SOK.Application.Services.Implementation;
using SOK.Application.Services.Interface;
using SOK.Infrastructure.Persistence.Context;
using SOK.Infrastructure.Persistence.Seeding;
using SOK.Infrastructure.Repositories;

namespace SOK.Infrastructure.Extensions
{
    /// <summary>
    /// Zapewnia metody do rejestrowania wszelakich elementów aplikacji w kontenerze DI.
    /// </summary>
    public static class RegisterExtensions
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

        /// <summary>
        /// Rejestruje wszystkie potrzebne usługi związane z obsługą warstwy domeny.
        /// </summary>
        /// <param name="configuration">Obiekt, reprezentujący konfigurację aplikacji.</param>
        /// <returns>
        /// Obiekt <see cref="IServiceCollection"/> do tworzenia łańcuchu wywołań.
        /// </returns>
        public static IServiceCollection RegisterServices(this IServiceCollection services)
        {
            // Rejestracja IParishInfoService
            services.AddScoped<IParishInfoService, ParishInfoService>();

            // Rejestracja ISubmissionService
            services.AddScoped<ISubmissionService, SubmissionService>();

            return services;
        }
    }
}
