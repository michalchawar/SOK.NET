using SOK.Application.Common.DTO;
using SOK.Domain.Entities.Parish;

namespace SOK.Application.Services.Interface
{
    /// <summary>
    /// Serwis do wysyłania gotowych maili.
    /// </summary>
    public interface IEmailNotificationService
    {
        /// <summary>
        /// Wysyła mail potwierdzający dla danego zgłoszenia.
        /// </summary>
        /// <param name="submission">Zgłoszenie.</param>
        /// <returns>
        /// Obiekt <see cref="Task"/>, reprezentujący asynchroniczną operację, 
        /// zawierający wartość bool wskazującą, czy mail został wysłany pomyślnie.
        /// </returns>
        Task<bool> SendConfirmationEmail(Submission submission, bool onlyQueue = false);

        /// <summary>
        /// Wysyła mail potwierdzający dla danego zgłoszenia.
        /// </summary>
        /// <param name="submissionId">Identyfikator zgłoszenia.</param>
        /// <returns>
        /// Obiekt <see cref="Task"/>, reprezentujący asynchroniczną operację, 
        /// zawierający wartość bool wskazującą, czy mail został wysłany pomyślnie.
        /// </returns>
        Task<bool> SendConfirmationEmail(int submissionId, bool onlyQueue = false);
        
        /// <summary>
        /// Wysyła mail potwierdzający dla danego zgłoszenia.
        /// </summary>
        /// <param name="submission">Zgłoszenie.</param>
        /// <returns>
        /// Obiekt <see cref="Task"/>, reprezentujący asynchroniczną operację, 
        /// zawierający wartość bool wskazującą, czy mail został wysłany pomyślnie.
        /// </returns>
        Task<bool> SendVisitPlannedEmail(Submission submission, bool onlyQueue = false);

        /// <summary>
        /// Wysyła mail potwierdzający dla danego zgłoszenia.
        /// </summary>
        /// <param name="submissionId">Identyfikator zgłoszenia.</param>
        /// <returns>
        /// Obiekt <see cref="Task"/>, reprezentujący asynchroniczną operację, 
        /// zawierający wartość bool wskazującą, czy mail został wysłany pomyślnie.
        /// </returns>
        Task<bool> SendVisitPlannedEmail(int submissionId, bool onlyQueue = false);
    }
}
