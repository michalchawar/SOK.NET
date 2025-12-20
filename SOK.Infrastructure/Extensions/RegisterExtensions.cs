using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SOK.Application.Common.Interface;
using SOK.Application.Services.Implementation;
using SOK.Application.Services.Interface;
using SOK.Infrastructure.Persistence.Context;
using SOK.Infrastructure.Persistence.Seeding;
using SOK.Infrastructure.Repositories;
using SOK.Infrastructure.Services;

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
            // Rejestracja HttpContextAccessor (potrzebny do śledzenia autora zmian w snapshotach)
            services.AddHttpContextAccessor();

            // Rejestracja usług
            services.AddScoped<IParishInfoService, ParishInfoService>();
            services.AddScoped<ISubmissionService, SubmissionService>();
            services.AddScoped<IScheduleService, ScheduleService>();
            services.AddScoped<IPlanService, PlanService>();
            services.AddScoped<IParishMemberService, ParishMemberService>();
            services.AddScoped<ISubmitterService, SubmitterService>();
            services.AddScoped<IBuildingService, BuildingService>();
            services.AddScoped<IStreetService, StreetService>();
            services.AddScoped<ICityService, CityService>();
            services.AddScoped<IBuildingAssignmentService, BuildingAssignmentService>();
            services.AddScoped<IAgendaService, AgendaService>();
            services.AddScoped<IVisitService, VisitService>();
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<IEmailNotificationService, EmailNotificationService>();

            // Rejestracja serwisu w tle do wysyłania emaili
            services.AddHostedService<EmailSenderBackgroundService>();

            return services;
        }
    }
}
