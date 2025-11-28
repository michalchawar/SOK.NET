using SOK.Domain.Entities.Parish;

namespace SOK.Application.Common.Helpers.EmailTypes
{
    /// <summary>
    /// Email powiadamiający o nieprawidłowym adresie email
    /// (do wysyłania manualnego przez administratora)
    /// </summary>
    public class InvalidEmailNotification : EmailTypeBase
    {
        private readonly Submission _submission;
        private readonly string _newEmail;

        public override string TemplateName => "invalid_email";
        public override string DefaultSubject => "Nieprawidłowy adres e-mail w zgłoszeniu";
        public override int Priority => 5;
        public override string To => _newEmail;

        /// <summary>
        /// Tworzy powiadomienie o nieprawidłowym emailu
        /// </summary>
        /// <param name="submission">Zgłoszenie</param>
        /// <param name="invalidEmail">Nieprawidłowy adres email</param>
        public InvalidEmailNotification(Submission submission, string newEmail)
        {
            _submission = submission ?? throw new ArgumentNullException(nameof(submission));
            _newEmail = newEmail ?? throw new ArgumentNullException(nameof(newEmail));
        }

        public override Dictionary<string, string> BuildTemplateData()
        {
            if (_submission.Submitter == null)
                throw new InvalidOperationException("Submission.Submitter must be loaded");
            if (_submission.Address == null)
                throw new InvalidOperationException("Submission.Address must be loaded");

            return new Dictionary<string, string>
            {
                ["submitter_name"] = _submission.Submitter.Name,
                ["submitter_surname"] = _submission.Submitter.Surname,
                ["submission_uid"] = _submission.UniqueId.ToString(),
                ["invalid_email"] = _submission.Submitter.Email!,
                ["new_email"] = _newEmail,
                ["address"] = FormatAddress()
            };
        }

        private string FormatAddress()
        {
            var address = _submission.Address;
            var building = address.Building;
            var street = building?.Street;
            var type = street?.Type;

            if (building == null || street == null)
                return "Adres nieokreślony";

            return $"{type?.FullName ?? ""} {street.Name} {building.Number}{building.Letter}/{address.ApartmentNumber}{address.ApartmentLetter}".Trim();
        }
    }
}
