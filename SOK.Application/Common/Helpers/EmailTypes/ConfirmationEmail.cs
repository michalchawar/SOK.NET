using SOK.Domain.Entities.Parish;

namespace SOK.Application.Common.Helpers.EmailTypes
{
    /// <summary>
    /// Email potwierdzający przyjęcie zgłoszenia
    /// </summary>
    public class ConfirmationEmail : EmailTypeBase
    {
        private readonly Submission _submission;
        private readonly string _controlLinkBase;

        public override string TemplateName => "confirmation";
        public override string DefaultSubject => "Potwierdzenie przyjęcia zgłoszenia";
        public override string To => _submission.Submitter.Email!;
        public override int Priority => 7;

        /// <summary>
        /// Tworzy email potwierdzający dla zgłoszenia
        /// </summary>
        /// <param name="submission">Zgłoszenie (z załadowanymi relacjami: Submitter, Address, Visit.Schedule)</param>
        /// <param name="controlLinkBase">Bazowy URL aplikacji (np. "https://domena.pl")</param>
        public ConfirmationEmail(Submission submission, string controlLinkBase)
        {
            _submission = submission ?? throw new ArgumentNullException(nameof(submission));
            _controlLinkBase = controlLinkBase ?? throw new ArgumentNullException(nameof(controlLinkBase));
        }

        public override Dictionary<string, string> BuildTemplateData()
        {
            // Walidacja wymaganych relacji
            if (_submission.Submitter == null)
                throw new InvalidOperationException("Submission.Submitter must be loaded");
            if (_submission.Address == null)
                throw new InvalidOperationException("Submission.Address must be loaded");

            var address = _submission.Address;
            var building = address.Building;
            var street = building?.Street;

            return new Dictionary<string, string>
            {
                ["subject"] = GetSubject(),
                ["control_link"] = $"{_controlLinkBase}?submissionUid={_submission.UniqueId}&accessToken={_submission.AccessToken}",
            };
        }
    }
}
