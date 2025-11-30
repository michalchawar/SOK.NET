using System.Net;
using System.Net.Mail;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using SOK.Application.Common.Helpers;
using SOK.Application.Common.Helpers.EmailTypes;
using SOK.Application.Common.Interface;
using SOK.Application.Services.Interface;
using SOK.Domain.Entities.Parish;

namespace SOK.Application.Services.Implementation
{
    /// <summary>
    /// Serwis do wysyłania emaili z kolejkowaniem
    /// </summary>
    public class EmailService : IEmailService
    {
        private readonly IUnitOfWorkParish _uow;
        private readonly IParishInfoService _parishInfoService;
        private readonly EmailTemplateService _templateService;
        private readonly ILogger<EmailService> _logger;
        private int _timeoutMs = 30000; // Domyślny timeout SMTP w milisekundach

        public EmailService(
            IUnitOfWorkParish uow,
            IParishInfoService parishInfoService,
            ILogger<EmailService> logger)
        {
            _uow = uow;
            _parishInfoService = parishInfoService;
            _templateService = new EmailTemplateService();
            _logger = logger;
        }

        public void SetSMTPTimeout(int timeoutMs)
        {
            if (timeoutMs <= 0)
                throw new ArgumentOutOfRangeException(nameof(timeoutMs), "Timeout musi być większy niż 0 ms.");

            _timeoutMs = timeoutMs;
        }

        /// <inheritdoc />
        public async Task<int?> QueueEmailAsync(
            EmailTypeBase emailType,
            Submission submission,
            List<string>? cc = null,
            List<string>? bcc = null,
            bool forceSend = true)
        {
            if (emailType == null)
                throw new ArgumentNullException(nameof(emailType));

            // Automatycznie buduj dane z emailType
            var templateData = emailType.BuildTemplateData();
            var subject = emailType.GetSubject();
            var templateName = emailType.TemplateName;
            var priority = emailType.Priority;

            // Wywołaj główną metodę kolejkowania
            return await QueueEmailHelperAsync(
                to: emailType.To,
                subject: subject,
                templateName: templateName,
                templateData: templateData,
                priority: priority,
                submission: submission,
                cc: cc,
                bcc: bcc,
                forceSend: forceSend
            );
        }

        /// <inheritdoc />
        public async Task<string> PreviewEmailAsync(EmailTypeBase emailType)
        {
            if (emailType == null)
                throw new ArgumentNullException(nameof(emailType));

            // Automatycznie buduj dane z emailType
            var templateData = emailType.BuildTemplateData();

            // Wzbogać dane o dane parafii
            templateData = await EnrichTemplateDataAsync(templateData);

            // Wygeneruj HTML
            return await _templateService.BuildEmailAsync(emailType.TemplateName, templateData);
        }

        /// <inheritdoc />
        protected async Task<int?> QueueEmailHelperAsync(
            string to,
            string subject,
            string templateName,
            Dictionary<string, string> templateData,
            Submission submission,
            int priority = 5,
            List<string>? cc = null,
            List<string>? bcc = null,
            bool forceSend = true)
        {
            try
            {
                // Walidacja adresu email
                if (!IsValidEmail(to))
                {
                    _logger.LogWarning($"Invalid email address: {to}");
                    return null;
                }

                // Normalizacja priorytetu
                if (priority > 10) priority = 10;
                if (priority < 0) priority = 0;

                // Pobierz ustawienia SMTP
                var smtpSettings = await GetSmtpSettingsAsync();
                if (smtpSettings == null)
                {
                    _logger.LogError("SMTP settings not configured");
                    return null;
                }

                // Dodaj dane parafii do template data
                templateData = await EnrichTemplateDataAsync(templateData);

                // Generuj treść emaila
                string content = await _templateService.BuildEmailAsync(templateName, templateData);

                // Sprawdź czy to duplikat
                if (await IsDuplicateEmailAsync(to, templateName, content))
                {
                    _logger.LogInformation($"Duplicate email prevented for {to}, template: {templateName}");
                    return null;
                }

                // Pobierz globalnych odbiorców BCC z ustawień
                var globalBccString = await _parishInfoService.GetValueAsync(InfoKeys.Email.BccRecipients);
                var globalBccList = new List<string>();
                if (!string.IsNullOrWhiteSpace(globalBccString))
                {
                    // Rozdziel po średniku i oczyść białe znaki
                    globalBccList = globalBccString
                        .Split(';', StringSplitOptions.RemoveEmptyEntries)
                        .Select(email => email.Trim())
                        .Where(email => IsValidEmail(email))
                        .ToList();
                }

                // Połącz przekazane BCC z globalnymi BCC
                var allBcc = (bcc ?? new List<string>()).Concat(globalBccList).Distinct().ToList();

                // Przygotuj listy odbiorców w formacie JSON
                var receiverList = new List<EmailRecipient> { new EmailRecipient { Email = to, Name = "" } };
                var ccList = cc?.Select(email => new EmailRecipient { Email = email, Name = "" }).ToList() ?? new List<EmailRecipient>();
                var bccList = allBcc.Select(email => new EmailRecipient { Email = email, Name = "" }).ToList();

                // Utwórz EmailLog
                var emailLog = new EmailLog
                {
                    SenderMail = smtpSettings.SenderEmail,
                    SenderName = smtpSettings.SenderName,
                    Receiver = JsonSerializer.Serialize(receiverList),
                    Cc = JsonSerializer.Serialize(ccList),
                    Bcc = JsonSerializer.Serialize(bccList),
                    Subject = subject,
                    Content = content,
                    Type = templateName,
                    Priority = priority,
                    Sent = false,
                    SubmissionId = submission.Id,
                };

                _uow.EmailLog.Add(emailLog);
                await _uow.SaveAsync();

                // Pobierz ID utworzonego rekordu
                var createdEmail = await _uow.EmailLog.GetAsync(
                    e => e.SenderMail == emailLog.SenderMail 
                      && e.Receiver == emailLog.Receiver 
                      && e.Subject == emailLog.Subject 
                      && e.Content == emailLog.Content
                      && !e.Sent);

                if (createdEmail != null)
                {
                    // Jeśli forceSend, próbuj wysłać od razu w tle
                    if (forceSend)
                    {
                        // Uruchom wysyłanie w tle, nie czekając na rezultat,
                        // aby uniknąć blokowania w przypadku problemów z serwerem SMTP.
                        _ = await SendEmailAsync(createdEmail.Id);
                    }

                    return createdEmail.Id;
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error queueing email to {to}");
                return null;
            }
        }

        /// <inheritdoc />
        public async Task<bool> SendEmailAsync(int emailLogId)
        {
            SmtpSettings? smtpSettings = null;
            try
            {
                var emailLog = await _uow.EmailLog.GetAsync(e => e.Id == emailLogId, tracked: true);
                if (emailLog == null)
                {
                    _logger.LogWarning($"EmailLog {emailLogId} not found");
                    return false;
                }

                if (emailLog.Sent)
                {
                    _logger.LogInformation($"EmailLog {emailLogId} already sent");
                    return true;
                }

                // Pobierz ustawienia SMTP
                smtpSettings = await GetSmtpSettingsAsync();
                if (smtpSettings == null)
                {
                    _logger.LogError("SMTP settings not configured");
                    return false;
                }

                _logger.LogInformation($"Attempting to send email {emailLogId} via SMTP: {smtpSettings.Host}:{smtpSettings.Port}, SSL: {smtpSettings.EnableSsl}, User: {smtpSettings.Username}");

                // Deserializuj odbiorców
                var receivers = JsonSerializer.Deserialize<List<EmailRecipient>>(emailLog.Receiver) ?? new List<EmailRecipient>();
                var ccReceivers = JsonSerializer.Deserialize<List<EmailRecipient>>(emailLog.Cc) ?? new List<EmailRecipient>();
                var bccReceivers = JsonSerializer.Deserialize<List<EmailRecipient>>(emailLog.Bcc) ?? new List<EmailRecipient>();

                // Utwórz wiadomość
                using var mailMessage = new MailMessage
                {
                    From = new MailAddress(emailLog.SenderMail, emailLog.SenderName),
                    Subject = emailLog.Subject,
                    Body = emailLog.Content,
                    IsBodyHtml = true
                };

                // Dodaj odbiorców
                foreach (var receiver in receivers)
                {
                    if (IsValidEmail(receiver.Email))
                        mailMessage.To.Add(new MailAddress(receiver.Email, receiver.Name));
                }

                foreach (var cc in ccReceivers)
                {
                    if (IsValidEmail(cc.Email))
                        mailMessage.CC.Add(new MailAddress(cc.Email, cc.Name));
                }

                foreach (var bcc in bccReceivers)
                {
                    if (IsValidEmail(bcc.Email))
                        mailMessage.Bcc.Add(new MailAddress(bcc.Email, bcc.Name));
                }

                if (mailMessage.To.Count == 0)
                {
                    _logger.LogWarning($"No valid recipients for EmailLog {emailLogId}");
                    return false;
                }

                // Konwertuj MailMessage na MimeMessage
                var mimeMessage = MimeMessage.CreateFromMailMessage(mailMessage);

                // Wyślij email używając MailKit
                using var smtpClient = new MailKit.Net.Smtp.SmtpClient();
                smtpClient.Timeout = this._timeoutMs;

                _logger.LogInformation($"Connecting to SMTP server {smtpSettings.Host}:{smtpSettings.Port} to send email with logId {emailLogId}...");
                try
                {
                    // Określ typ połączenia SSL na podstawie portu
                    var secureSocketOptions = smtpSettings.Port == 465 
                        ? SecureSocketOptions.SslOnConnect  // Implicit SSL dla portu 465
                        : (smtpSettings.EnableSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.None);

                    await smtpClient.ConnectAsync(smtpSettings.Host, smtpSettings.Port, secureSocketOptions);
                    
                    _logger.LogInformation($"Connected to SMTP. Server capabilities: {string.Join(", ", smtpClient.Capabilities)}");
                    _logger.LogInformation($"Authentication mechanisms: {string.Join(", ", smtpClient.AuthenticationMechanisms)}");

                    // Uwierzytelnienie jeśli podano dane logowania
                    if (!string.IsNullOrEmpty(smtpSettings.Username))
                    {
                        _logger.LogInformation($"Attempting authentication with username: {smtpSettings.Username}");
                        await smtpClient.AuthenticateAsync(smtpSettings.Username, smtpSettings.Password);
                        _logger.LogInformation($"Authentication successful");
                    }
                    else
                    {
                        _logger.LogInformation($"No username provided, skipping authentication");
                    }

                    await smtpClient.SendAsync(mimeMessage);
                    await smtpClient.DisconnectAsync(true);
                } 
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error during SendMailAsync for emailLogId {emailLogId}. SMTP Details - Host: {smtpSettings?.Host}, Port: {smtpSettings?.Port}, EnableSsl: {smtpSettings?.EnableSsl}");
                    return false;
                }

                // Zaktualizuj status
                emailLog.Sent = true;
                emailLog.SentTimestamp = DateTime.UtcNow;
                await _uow.SaveAsync();

                return true;
            }
            catch (SmtpException ex)
            {
                _logger.LogError(ex, $"SMTP error sending email {emailLogId}. StatusCode: {ex.StatusCode}, Host: {smtpSettings?.Host}:{smtpSettings?.Port}, SSL: {smtpSettings?.EnableSsl}");
                return false;
            }
            catch (System.Net.Sockets.SocketException ex)
            {
                _logger.LogError(ex, $"Socket error sending email {emailLogId}. ErrorCode: {ex.ErrorCode}, SocketErrorCode: {ex.SocketErrorCode}, Host: {smtpSettings?.Host}:{smtpSettings?.Port}");
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Unexpected error sending email {emailLogId}");
                return false;
            }
        }

        /// <inheritdoc />
        public async Task<int> SendPendingEmailsAsync(int maxAttempts = 300, int maxErrors = 5)
        {
            int sentCount = 0;
            int errorCount = 0;
            int attemptCount = 0;

            try
            {
                while (attemptCount < maxAttempts && errorCount < maxErrors)
                {
                    // Pobierz niewysłane emaile (sortowanie: priorytet DESC, data dodania ASC)
                    var pendingEmails = await _uow.EmailLog.GetAllAsync(
                        filter: e => !e.Sent,
                        tracked: false);

                    var pendingEmail = pendingEmails
                        .OrderByDescending(e => e.Priority)
                        .ThenBy(e => e.AddedTimestamp)
                        .FirstOrDefault();

                    if (pendingEmail == null)
                        break;

                    bool sent = await SendEmailAsync(pendingEmail.Id);
                    attemptCount++;

                    if (sent)
                    {
                        sentCount++;
                        errorCount = 0; // Reset licznika błędów
                    }
                    else
                    {
                        errorCount++;
                    }

                    // Krótka przerwa między wysyłkami, żeby nie przeciążać serwera
                    await Task.Delay(1000);
                }

                _logger.LogInformation($"Sent {sentCount} emails, {errorCount} errors, {attemptCount} attempts");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in SendPendingEmailsAsync");
            }

            return sentCount;
        }

        /// <inheritdoc />
        public async Task<bool> IsDuplicateEmailAsync(string to, string templateName, string content)
        {
            var existing = await _uow.EmailLog.GetAsync(
                e => e.Receiver.Contains(to) 
                  && e.Type == templateName 
                  && e.Content == content);

            return existing != null;
        }

        /// <summary>
        /// Pobiera ustawienia SMTP z bazy danych
        /// </summary>
        private async Task<SmtpSettings?> GetSmtpSettingsAsync()
        {
            try
            {
                var settings = await _parishInfoService.GetValuesAsync(new[]
                {
                    InfoKeys.Email.SmtpServer,
                    InfoKeys.Email.SmtpPort,
                    InfoKeys.Email.SmtpUserName,
                    InfoKeys.Email.SmtpPassword,
                    InfoKeys.Email.SmtpEnableSsl,
                    InfoKeys.Email.SenderEmail,
                    InfoKeys.Email.SenderName
                });

                if (!settings.ContainsKey(InfoKeys.Email.SmtpServer) || string.IsNullOrEmpty(settings[InfoKeys.Email.SmtpServer]))
                    return null;

                return new SmtpSettings
                {
                    Host = settings[InfoKeys.Email.SmtpServer],
                    Port = int.TryParse(settings.GetValueOrDefault(InfoKeys.Email.SmtpPort), out int port) ? port : 587,
                    Username = settings.GetValueOrDefault(InfoKeys.Email.SmtpUserName) ?? "",
                    Password = settings.GetValueOrDefault(InfoKeys.Email.SmtpPassword) ?? "",
                    EnableSsl = bool.TryParse(settings.GetValueOrDefault(InfoKeys.Email.SmtpEnableSsl), out bool ssl) && ssl,
                    SenderEmail = settings.GetValueOrDefault(InfoKeys.Email.SenderEmail) ?? "",
                    SenderName = settings.GetValueOrDefault(InfoKeys.Email.SenderName) ?? ""
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading SMTP settings");
                return null;
            }
        }

        /// <summary>
        /// Wzbogaca dane szablonu o informacje o parafii
        /// </summary>
        private async Task<Dictionary<string, string>> EnrichTemplateDataAsync(Dictionary<string, string> data)
        {
            var enriched = new Dictionary<string, string>(data);

            try
            {
                var parishInfo = await _parishInfoService.GetDictionaryAsync();

                // Dodaj podstawowe dane parafii, jeśli nie są już w danych
                if (!enriched.ContainsKey("parish_name"))
                    enriched["parish_name"] = parishInfo.GetValueOrDefault(InfoKeys.Parish.ShortName) ?? "";

                if (!enriched.ContainsKey("parish_address"))
                {
                    string address = $"{parishInfo.GetValueOrDefault(InfoKeys.Contact.StreetAndBuilding)}, " +
                                   $"{parishInfo.GetValueOrDefault(InfoKeys.Contact.PostalCode)} {parishInfo.GetValueOrDefault(InfoKeys.Contact.CityName)}";
                    enriched["parish_address"] = address;
                }

                if (!enriched.ContainsKey("parish_email"))
                    enriched["parish_email"] = parishInfo.GetValueOrDefault(InfoKeys.Contact.Email) ?? "";
                
                if (!enriched.ContainsKey("parish_phone"))
                    enriched["parish_phone"] = parishInfo.GetValueOrDefault(InfoKeys.Contact.MainPhone) ?? "";
                
                if (!enriched.ContainsKey("parish_website"))
                    enriched["parish_website"] = parishInfo.GetValueOrDefault(InfoKeys.Contact.Website) ?? "";

                if (!enriched.ContainsKey("subject"))
                    enriched["subject"] = "";
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error enriching template data with parish info");
            }

            return enriched;
        }

        /// <summary>
        /// Waliduje adres email
        /// </summary>
        private bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                var addr = new MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Klasa pomocnicza dla odbiorców emaila
        /// </summary>
        private class EmailRecipient
        {
            public string Email { get; set; } = string.Empty;
            public string Name { get; set; } = string.Empty;
        }

        /// <summary>
        /// Klasa pomocnicza dla ustawień SMTP
        /// </summary>
        private class SmtpSettings
        {
            public string Host { get; set; } = string.Empty;
            public int Port { get; set; }
            public string Username { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty;
            public bool EnableSsl { get; set; }
            public string SenderEmail { get; set; } = string.Empty;
            public string SenderName { get; set; } = string.Empty;
        }
    }
}
