using SOK.Application.Common.DTO;
using SOK.Application.Common.Helpers.EmailTypes;
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
        /// <param name="onlyQueue">Czy tylko dodać do kolejki, bez natychmiastowego wysyłania.</param>
        /// <returns>
        /// Obiekt <see cref="Task"/>, reprezentujący asynchroniczną operację, 
        /// zawierający wartość bool wskazującą, czy mail został wysłany pomyślnie.
        /// </returns>
        Task<bool> SendConfirmationEmail(Submission submission, bool onlyQueue = false);

        /// <summary>
        /// Wysyła mail potwierdzający dla danego zgłoszenia.
        /// </summary>
        /// <param name="submissionId">Identyfikator zgłoszenia.</param>
        /// <param name="onlyQueue">Czy tylko dodać do kolejki, bez natychmiastowego wysyłania.</param>
        /// <returns>
        /// Obiekt <see cref="Task"/>, reprezentujący asynchroniczną operację, 
        /// zawierający wartość bool wskazującą, czy mail został wysłany pomyślnie.
        /// </returns>
        Task<bool> SendConfirmationEmail(int submissionId, bool onlyQueue = false);

        /// <summary>
        /// Podgląd maila potwierdzającego dla danego zgłoszenia.
        /// </summary>
        /// <param name="submissionId">Identyfikator zgłoszenia.</param>
        /// <returns></returns>
        Task<string> PreviewConfirmationEmail(int submissionId);
        
        /// <summary>
        /// Wysyła mail potwierdzający dla danego zgłoszenia.
        /// </summary>
        /// <param name="submission">Zgłoszenie.</param>
        /// <param name="onlyQueue">Czy tylko dodać do kolejki, bez natychmiastowego wysyłania.</param>
        /// <returns>
        /// Obiekt <see cref="Task"/>, reprezentujący asynchroniczną operację, 
        /// zawierający wartość bool wskazującą, czy mail został wysłany pomyślnie.
        /// </returns>
        Task<bool> SendVisitPlannedEmail(Submission submission, bool onlyQueue = false);

        /// <summary>
        /// Wysyła mail potwierdzający dla danego zgłoszenia.
        /// </summary>
        /// <param name="submissionId">Identyfikator zgłoszenia.</param>
        /// <param name="onlyQueue">Czy tylko dodać do kolejki, bez natychmiastowego wysyłania.</param>
        /// <returns>
        /// Obiekt <see cref="Task"/>, reprezentujący asynchroniczną operację, 
        /// zawierający wartość bool wskazującą, czy mail został wysłany pomyślnie.
        /// </returns>
        Task<bool> SendVisitPlannedEmail(int submissionId, bool onlyQueue = false);

        /// <summary>
        /// Podgląd maila informującego o zaplanowanej wizycie dla danego zgłoszenia.
        /// </summary>
        /// <param name="submissionId">Identyfikator zgłoszenia.</param>
        /// <returns></returns>
        Task<string> PreviewVisitPlannedEmail(int submissionId);
        
        /// <summary>
        /// Wysyła mail z zapytaniem o poprawność manualnie wprowadzonego adresu mailowego.
        /// </summary>
        /// <param name="submission">Zgłoszenie.</param>
        /// <param name="to">Adresat maila.</param>
        /// <param name="onlyQueue">Czy tylko dodać do kolejki, bez natychmiastowego wysyłania.</param>
        /// <returns>
        /// Obiekt <see cref="Task"/>, reprezentujący asynchroniczną operację, 
        /// zawierający wartość bool wskazującą, czy mail został wysłany pomyślnie.
        /// </returns>
        Task<bool> SendInvalidEmailNotification(Submission submission, string to, bool onlyQueue = false);

        /// <summary>
        /// Wysyła mail z zapytaniem o poprawność manualnie wprowadzonego adresu mailowego.
        /// </summary>
        /// <param name="submissionId">Identyfikator zgłoszenia.</param>
        /// <param name="to">Adresat maila.</param>
        /// <param name="onlyQueue">Czy tylko dodać do kolejki, bez natychmiastowego wysyłania.</param>
        /// <returns>
        /// Obiekt <see cref="Task"/>, reprezentujący asynchroniczną operację, 
        /// zawierający wartość bool wskazującą, czy mail został wysłany pomyślnie.
        /// </returns>
        Task<bool> SendInvalidEmailNotification(int submissionId, string to, bool onlyQueue = false);

        /// <summary>
        /// Podgląd maila informującego o nieprawidłowym emailu dla danego zgłoszenia.
        /// </summary>
        /// <param name="submissionId">Identyfikator zgłoszenia.</param>
        /// <param name="to">Adresat maila.</param>
        /// <returns></returns>
        Task<string> PreviewInvalidEmailNotification(int submissionId, string to);

        /// <summary>
        /// Podgląd maila z listą zmian w zgłoszeniu.
        /// </summary>
        /// <param name="submissionId">Identyfikator zgłoszenia.</param>
        /// <param name="changes">Obiekt reprezentujący zmiany zgłoszenia.</param>
        /// <returns></returns>
        Task<string> PreviewDataChangeEmail(int submissionId, DataChanges changes);
    }
}
