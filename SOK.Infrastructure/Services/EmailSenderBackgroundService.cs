using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SOK.Application.Common.Interface;
using SOK.Application.Services.Interface;
using SOK.Infrastructure.Persistence.Context;

namespace SOK.Infrastructure.Services
{
    /// <summary>
    /// Serwis w tle wysyłający oczekujące emaile co godzinę dla wszystkich parafii
    /// </summary>
    public class EmailSenderBackgroundService : BackgroundService
    {
        private readonly ILogger<EmailSenderBackgroundService> _logger;
        private readonly IServiceScopeFactory _scopeFactory;

        public EmailSenderBackgroundService(
            ILogger<EmailSenderBackgroundService> logger,
            IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("EmailSenderBackgroundService started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _logger.LogInformation("EmailSenderBackgroundService: Starting email send cycle for all parishes");

                    int totalSentCount = 0;

                    using (var scope = _scopeFactory.CreateScope())
                    {
                        var uowCentral = scope.ServiceProvider.GetRequiredService<IUnitOfWorkCentral>();
                        var cryptoService = scope.ServiceProvider.GetRequiredService<ICryptoService>();

                        // Pobierz wszystkie parafie z centralnej bazy danych
                        var parishes = await uowCentral.Parishes.GetAllAsync();

                        _logger.LogInformation($"EmailSenderBackgroundService: Found {parishes.Count()} parishes");

                        // Dla każdej parafii osobno wyślij emaile
                        foreach (var parish in parishes)
                        {
                            try
                            {
                                _logger.LogInformation($"EmailSenderBackgroundService: Processing parish {parish.ParishName} (UID: {parish.UniqueId})");

                                // Odszyfruj connection string
                                string parishConnectionString = cryptoService.Decrypt(parish.EncryptedConnectionString);

                                // Utwórz nowy scope dla tej parafii
                                using (var parishScope = _scopeFactory.CreateScope())
                                {
                                    // Pobierz ParishDbContext i ustaw connection string
                                    var parishContext = parishScope.ServiceProvider.GetRequiredService<ParishDbContext>();
                                    parishContext.OverrideConnectionString = parishConnectionString;

                                    // Pobierz EmailService (który użyje kontekstu z ustawionym connection stringiem)
                                    var emailService = parishScope.ServiceProvider.GetRequiredService<IEmailService>();

                                    // Wyślij do 300 emaili, przerywając po 5 błędach pod rząd
                                    int sentCount = await emailService.SendPendingEmailsAsync(maxAttempts: 300, maxErrors: 5);

                                    totalSentCount += sentCount;

                                    _logger.LogInformation($"EmailSenderBackgroundService: Sent {sentCount} emails for parish {parish.ParishName}");
                                }
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, $"EmailSenderBackgroundService: Error processing parish {parish.ParishName} (UID: {parish.UniqueId})");
                            }
                        }
                    }

                    _logger.LogInformation($"EmailSenderBackgroundService: Completed email send cycle. Total sent: {totalSentCount} emails");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "EmailSenderBackgroundService: Error during email send cycle");
                }

                // Czekaj godzinę przed następną iteracją
                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }

            _logger.LogInformation("EmailSenderBackgroundService stopped");
        }
    }
}
