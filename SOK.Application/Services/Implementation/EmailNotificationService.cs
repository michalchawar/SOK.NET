using SOK.Application.Common.DTO;
using SOK.Application.Common.Helpers;
using SOK.Application.Common.Helpers.EmailTypes;
using SOK.Application.Common.Interface;
using SOK.Domain.Entities.Parish;

namespace SOK.Application.Services.Interface
{
    /// <summary>
    /// Serwis do wysyłania gotowych maili.
    /// </summary>
    public class EmailNotificationService : IEmailNotificationService
    {
        private readonly IUnitOfWorkParish _uow;
        private readonly IEmailService _emailService;

        private string ControlLinkBase = string.Empty;
        private bool SendEmails = false;

        public EmailNotificationService(IUnitOfWorkParish uow, IEmailService emailService)
        {
            _uow = uow;
            _emailService = emailService;
        }

        /// <inheritdoc />
        public async Task<bool> SendConfirmationEmail(Submission submission, bool onlyQueue = false)
        {
            if (string.IsNullOrEmpty(ControlLinkBase))
                await InitProperties();

            if (!SendEmails)
                return false;

            var email = new ConfirmationEmail(submission, ControlLinkBase);
            try
            {
                var id = await _emailService.QueueEmailAsync(
                    email,
                    submission,
                    forceSend: !onlyQueue
                );
                return id != null;
            } 
            catch (Exception)
            {
                // Log the exception as needed
                return false;
            }
        }

        /// <inheritdoc />
        public async Task<bool> SendConfirmationEmail(int submissionId, bool onlyQueue = false)
        {
            var submission = await _uow.Submission.GetAsync(
                filter: s => s.Id == submissionId
            );

            if (submission == null)
                throw new InvalidOperationException($"Submission with ID {submissionId} not found.");

            return await SendConfirmationEmail(submission, onlyQueue);
        }
        
        /// <inheritdoc />
        public async Task<bool> SendVisitPlannedEmail(Submission submission, bool onlyQueue = false)
        {
            if (string.IsNullOrEmpty(ControlLinkBase))
                await InitProperties();

            if (!SendEmails)
                return false;

            var email = new VisitPlannedEmail(submission, ControlLinkBase);
            try
            {
                var id = await _emailService.QueueEmailAsync(
                    email,
                    submission,
                    forceSend: !onlyQueue
                );
                return id != null;
            } 
            catch (Exception)
            {
                // Log the exception as needed
                return false;
            }
        }

        /// <inheritdoc />
        public async Task<bool> SendVisitPlannedEmail(int submissionId, bool onlyQueue = false)
        {
            var submission = await _uow.Submission.GetAsync(
                filter: s => s.Id == submissionId
            );

            if (submission == null)
                throw new InvalidOperationException($"Submission with ID {submissionId} not found.");

            return await SendVisitPlannedEmail(submission, onlyQueue);
        }

        private async Task InitProperties()
        {
            if (string.IsNullOrEmpty(ControlLinkBase))
            {
                ControlLinkBase = await _uow.ParishInfo.GetValueAsync(InfoKeys.EmbeddedApplication.ControlPanelBaseUrl) ?? string.Empty;
                SendEmails = (await _uow.ParishInfo.GetValueAsync(InfoKeys.Email.EnableEmailSending)) == "true";
            }
        }
    }
}
