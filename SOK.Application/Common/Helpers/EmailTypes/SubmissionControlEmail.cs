using SOK.Domain.Entities.Parish;

namespace SOK.Application.Common.Helpers.EmailTypes
{
    /// <summary>
    /// Email z linkiem do panelu kontrolnego zgłoszenia
    /// </summary>
    public class SubmissionControlEmail : EmailTypeBase
    {
        private readonly Submission _submission;
        private readonly string _controlLinkBase;

        public override string TemplateName => "submission_control";
        public override string DefaultSubject => "Link do zarządzania zgłoszeniem";
        public override string To => _submission.Submitter.Email!;
        public override int Priority => 5;

        /// <summary>
        /// Tworzy email z linkiem kontrolnym
        /// </summary>
        /// <param name="submission">Zgłoszenie</param>
        /// <param name="controlLinkBase">Bazowy URL aplikacji</param>
        public SubmissionControlEmail(Submission submission, string controlLinkBase)
        {
            _submission = submission ?? throw new ArgumentNullException(nameof(submission));
            _controlLinkBase = controlLinkBase ?? throw new ArgumentNullException(nameof(controlLinkBase));
        }

        public override Dictionary<string, string> BuildTemplateData()
        {
            if (_submission.Submitter == null)
                throw new InvalidOperationException("Submission.Submitter must be loaded");

            return new Dictionary<string, string>
            {
                ["submitter_name"] = _submission.Submitter.Name,
                ["submitter_surname"] = _submission.Submitter.Surname,
                ["control_link"] = $"{_controlLinkBase}/submission/{_submission.UniqueId}?token={_submission.AccessToken}",
                ["access_token"] = _submission.AccessToken
            };
        }
    }
}
