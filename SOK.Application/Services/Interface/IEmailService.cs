using SOK.Application.Common.Helpers.EmailTypes;
using SOK.Domain.Entities.Parish;

namespace SOK.Application.Services.Interface
{
    /// <summary>
    /// Interfejs serwisu do wysyłania emaili
    /// </summary>
    public interface IEmailService
    {
        /// <summary>
        /// Ustawia timeout dla połączeń SMTP w milisekundach
        /// </summary>
        /// <param name="timeoutMs">Timeout w milisekundach</param>
        void SetSMTPTimeout(int timeoutMs);

        /// <summary>
        /// Kolejkuje email do wysłania używając typu emaila
        /// </summary>
        /// <param name="emailType">Typ emaila z automatycznym wypełnianiem danych</param>
        /// <param name="submission">Zgłoszenie do powiązania z emailem</param>
        /// <param name="cc">Lista adresów CC</param>
        /// <param name="bcc">Lista adresów BCC</param>
        /// <param name="forceSend">Czy próbować wysłać od razu (domyślnie true)</param>
        /// <returns>ID utworzonego EmailLog lub null jeśli nie udało się utworzyć</returns>
        Task<int?> QueueEmailAsync(
            EmailTypeBase emailType,
            Submission submission,
            List<string>? cc = null,
            List<string>? bcc = null,
            bool forceSend = true);

        /// <summary>
        /// Kolejkuje email do wysłania (metoda niskopoziomowa)
        /// </summary>
        /// <param name="to">Adres odbiorcy</param>
        /// <param name="subject">Temat emaila</param>
        /// <param name="templateName">Nazwa szablonu (np. "confirmation")</param>
        /// <param name="templateData">Dane do podstawienia w szablonie</param>
        /// <param name="priority">Priorytet wysyłki (0-10)</param>
        /// <param name="submissionId">ID powiązanego zgłoszenia (opcjonalne)</param>
        /// <param name="cc">Lista adresów CC</param>
        /// <param name="bcc">Lista adresów BCC</param>
        /// <param name="forceSend">Czy próbować wysłać od razu (domyślnie true)</param>
        /// <returns>ID utworzonego EmailLog lub null jeśli nie udało się utworzyć</returns>
        // protected Task<int?> QueueEmailAsync(
        //     string to,
        //     string subject,
        //     string templateName,
        //     Dictionary<string, string> templateData,
        //     Submission submission,
        //     int priority = 5,
        //     List<string>? cc = null,
        //     List<string>? bcc = null,
        //     bool forceSend = true);

        /// <summary>
        /// Próbuje wysłać email z kolejki
        /// </summary>
        /// <param name="emailLogId">ID EmailLog do wysłania</param>
        /// <returns>True jeśli wysłano pomyślnie</returns>
        Task<bool> SendEmailAsync(int emailLogId);

        /// <summary>
        /// Wysyła wszystkie oczekujące emaile z kolejki
        /// </summary>
        /// <param name="maxAttempts">Maksymalna liczba emaili do wysłania</param>
        /// <param name="maxErrors">Maksymalna liczba błędów pod rząd przed przerwaniem</param>
        /// <returns>Liczba wysłanych emaili</returns>
        Task<int> SendPendingEmailsAsync(int maxAttempts = 300, int maxErrors = 5);

        /// <summary>
        /// Generuje podgląd emaila bez wysyłania
        /// </summary>
        /// <param name="templateName">Nazwa szablonu</param>
        /// <param name="templateData">Dane do podstawienia</param>
        /// <returns>HTML emaila</returns>
        Task<string> PreviewEmailAsync(EmailTypeBase emailType);

        /// <summary>
        /// Sprawdza czy email o podanych parametrach został już wysłany (duplikat)
        /// </summary>
        /// <param name="to">Odbiorca</param>
        /// <param name="templateName">Typ szablonu</param>
        /// <param name="content">Treść emaila</param>
        /// <returns>True jeśli email już istnieje w bazie</returns>
        Task<bool> IsDuplicateEmailAsync(string to, string templateName, string content);
    }
}
