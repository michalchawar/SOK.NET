using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger _logger;
        private readonly IUnitOfWorkParish _uow;
        private readonly IEmailService _emailService;

        private string ControlLinkBase = string.Empty;
        private bool SendEmails = false;

        public EmailNotificationService(IUnitOfWorkParish uow, IEmailService emailService, ILogger<EmailNotificationService> logger)
        {
            _uow = uow;
            _emailService = emailService;
            _logger = logger;
        }

        /// <inheritdoc />
        public async Task<bool> SendConfirmationEmail(Submission submission, bool onlyQueue = false)
        {
            var email = new ConfirmationEmail(submission, ControlLinkBase);
            return await HandleEmailSending(email, submission, onlyQueue);
        }

        /// <inheritdoc />
        public async Task<bool> SendConfirmationEmail(int submissionId, bool onlyQueue = false)
        {
            var submission = await _uow.Submission.GetAsync(
                filter: s => s.Id == submissionId,
                includeProperties: "Submitter"
            );

            if (submission == null)
                throw new InvalidOperationException($"Submission with ID {submissionId} not found.");

            return await SendConfirmationEmail(submission, onlyQueue);
        }
        
        /// <inheritdoc />
        public async Task<string> PreviewConfirmationEmail(int submissionId)
        {
            var submission = await _uow.Submission.GetAsync(
                filter: s => s.Id == submissionId,
                includeProperties: "Submitter"
            );

            if (submission == null)
                throw new InvalidOperationException($"Submission with ID {submissionId} not found.");

            var email = new ConfirmationEmail(submission, ControlLinkBase);

            return await RequestEmailPreview(email);
        }

        /// <inheritdoc />
        public async Task<bool> SendVisitPlannedEmail(Submission submission, bool onlyQueue = false)
        {
            var email = new VisitPlannedEmail(submission, ControlLinkBase);
            return await HandleEmailSending(email, submission, onlyQueue);
        }

        /// <inheritdoc />
        public async Task<bool> SendVisitPlannedEmail(int submissionId, bool onlyQueue = false)
        {
            var submission = await _uow.Submission.GetAsync(
                filter: s => s.Id == submissionId,
                includeProperties: "Submitter"
            );

            if (submission == null)
                throw new InvalidOperationException($"Submission with ID {submissionId} not found.");

            return await SendVisitPlannedEmail(submission, onlyQueue);
        }
        
        /// <inheritdoc />
        public async Task<string> PreviewVisitPlannedEmail(int submissionId)
        {
            var submission = await _uow.Submission.GetAsync(
                filter: s => s.Id == submissionId,
                includeProperties: "Submitter"
            );

            if (submission == null)
                throw new InvalidOperationException($"Submission with ID {submissionId} not found.");

            var email = new VisitPlannedEmail(submission, ControlLinkBase);

            return await RequestEmailPreview(email);
        }
        
        /// <inheritdoc />
        public async Task<bool> SendInvalidEmailNotification(Submission submission, string to, bool onlyQueue = false)
        {
            if (string.IsNullOrWhiteSpace(to) || !Regex.IsMatch(to, RegExpressions.EmailPattern))
                return false;

            var email = new InvalidEmailNotification(submission, to);
            return await HandleEmailSending(email, submission, onlyQueue);
        }

        /// <inheritdoc />
        public async Task<bool> SendInvalidEmailNotification(int submissionId, string to, bool onlyQueue = false)
        {
            var submission = await _uow.Submission.GetAsync(
                filter: s => s.Id == submissionId,
                includeProperties: "Submitter,Address"
            );

            if (submission == null)
                throw new InvalidOperationException($"Submission with ID {submissionId} not found.");

            return await SendInvalidEmailNotification(submission, to, onlyQueue);
        }
        
        /// <inheritdoc />
        public async Task<string> PreviewInvalidEmailNotification(int submissionId, string to)
        {
            var submission = await _uow.Submission.GetAsync(
                filter: s => s.Id == submissionId,
                includeProperties: "Submitter,Address"
            );

            if (submission == null)
                throw new InvalidOperationException($"Submission with ID {submissionId} not found.");

            var email = new InvalidEmailNotification(submission, to);

            return await RequestEmailPreview(email);
        }
        
        /// <inheritdoc />
        public async Task<string> PreviewDataChangeEmail(int submissionId, DataChanges changes)
        {
            var submission = await _uow.Submission.GetAsync(
                filter: s => s.Id == submissionId,
                includeProperties: "Submitter"
            );

            if (submission == null)
                throw new InvalidOperationException($"Submission with ID {submissionId} not found.");

            var email = new DataChangeEmail(submission, ControlLinkBase, changes);

            return await RequestEmailPreview(email);
        }

        private async Task<bool> HandleEmailSending(EmailTypeBase email, Submission submission, bool onlyQueue)
        {
            if (string.IsNullOrEmpty(ControlLinkBase))
                await InitProperties();

            if (!SendEmails)
                return false;

            try
            {
                var id = await _emailService.QueueEmailAsync(
                    email,
                    submission,
                    forceSend: !onlyQueue
                );
                return id != null;
            } 
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error sending email");
                return false;
            }
        }

        private async Task<string> RequestEmailPreview(EmailTypeBase email)
        {
            if (string.IsNullOrEmpty(ControlLinkBase))
                await InitProperties();

            try
            {
                var previewHtml = await _emailService.PreviewEmailAsync(
                    email
                );

                return string.IsNullOrWhiteSpace(previewHtml) ? string.Empty : previewHtml;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error generating email preview");
                return string.Empty;
            }
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
