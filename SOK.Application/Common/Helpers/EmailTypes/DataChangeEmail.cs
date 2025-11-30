using SOK.Domain.Entities.Parish;

namespace SOK.Application.Common.Helpers.EmailTypes
{
    /// <summary>
    /// Email powiadamiający o zmianie danych zgłoszenia
    /// </summary>
    public class DataChangeEmail : EmailTypeBase
    {
        private readonly Submission _submission;
        private readonly string _controlLinkBase;
        private readonly string _changesDescription;

        public override string TemplateName => "data_change";
        public override string DefaultSubject => "Zmiana danych zgłoszenia";
        public override string To => _submission.Submitter.Email!;
        public override int Priority => 6;

        /// <summary>
        /// Tworzy email o zmianie danych
        /// </summary>
        /// <param name="submission">Zgłoszenie</param>
        /// <param name="controlLinkBase">Bazowy URL aplikacji</param>
        /// <param name="changesDescription">Opis wprowadzonych zmian (HTML)</param>
        public DataChangeEmail(Submission submission, string controlLinkBase, string? changesDescription = null)
        {
            _submission = submission ?? throw new ArgumentNullException(nameof(submission));
            _controlLinkBase = controlLinkBase ?? throw new ArgumentNullException(nameof(controlLinkBase));
            _changesDescription = changesDescription ?? "Dane zgłoszenia zostały zaktualizowane.";
        }

        public override Dictionary<string, string> BuildTemplateData()
        {
            if (_submission.Submitter == null)
                throw new InvalidOperationException("Submission.Submitter must be loaded");

            return new Dictionary<string, string>
            {
                ["subject"] = GetSubject(),
                ["submitter_name"] = _submission.Submitter.Name,
                ["submitter_surname"] = _submission.Submitter.Surname,
                ["submission_uid"] = _submission.UniqueId.ToString(),
                ["changes"] = _changesDescription,
                ["control_link"] = $"{_controlLinkBase}/submission/{_submission.UniqueId}?token={_submission.AccessToken}"
            };
        }
    }
}
