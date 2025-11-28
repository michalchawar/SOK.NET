using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SOK.Application.Services.Interface;

namespace SOK.Infrastructure.Services
{
    /// <summary>
    /// Serwis w tle wysyłający oczekujące emaile co godzinę
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
                    _logger.LogInformation("EmailSenderBackgroundService: Starting email send cycle");

                    using (var scope = _scopeFactory.CreateScope())
                    {
                        var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
                        
                        // Wyślij do 300 emaili, przerywając po 5 błędach pod rząd
                        int sentCount = await emailService.SendPendingEmailsAsync(maxAttempts: 300, maxErrors: 5);
                        
                        _logger.LogInformation($"EmailSenderBackgroundService: Sent {sentCount} emails");
                    }
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
