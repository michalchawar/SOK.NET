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
        private readonly DataChanges _changes;

        public override string TemplateName => "data_change";
        public override string DefaultSubject => "Aktualizacja zgłoszenia";
        public override string To => _submission.Submitter.Email!;
        public override int Priority => 6;

        /// <summary>
        /// Tworzy email o zmianie danych
        /// </summary>
        /// <param name="submission">Zgłoszenie (po zmianach)</param>
        /// <param name="controlLinkBase">Bazowy URL aplikacji</param>
        /// <param name="changes">Dane o zmianach (stare i nowe wartości)</param>
        public DataChangeEmail(Submission submission, string controlLinkBase, DataChanges changes)
        {
            _submission = submission ?? throw new ArgumentNullException(nameof(submission));
            _controlLinkBase = controlLinkBase ?? throw new ArgumentNullException(nameof(controlLinkBase));
            _changes = changes ?? throw new ArgumentNullException(nameof(changes));
        }

        public override Dictionary<string, string> BuildTemplateData()
        {
            if (_submission.Submitter == null)
                throw new InvalidOperationException("Submission.Submitter must be loaded");

            var data = new Dictionary<string, string>
            {
                ["subject"] = GetSubject(),
                ["control_link"] = $"{_controlLinkBase}?submissionUid={_submission.UniqueId}&accessToken={_submission.AccessToken}"
            };

            // Dodaj dane tylko jeśli się zmieniły i pogrub je w HTML
            AddChangedField(data, "name", "Imię i nazwisko", _changes.OldName, _changes.NewName);
            AddChangedField(data, "address", "Adres", _changes.OldAddress, _changes.NewAddress);
            AddChangedField(data, "email", "E-mail", _changes.OldEmail, _changes.NewEmail);
            
            // Notatki zgłaszającego - dodaj tylko jeśli któraś wartość nie jest pusta
            if (!string.IsNullOrEmpty(_changes.OldSubmitterNotes) || !string.IsNullOrEmpty(_changes.NewSubmitterNotes))
            {
                AddChangedField(data, "submitter_notes", "Dodatkowe uwagi", _changes.OldSubmitterNotes, _changes.NewSubmitterNotes);
                
                // Status notatek - dodaj tylko jeśli notatki istnieją i status się zmienił
                if (!string.IsNullOrEmpty(_changes.NewSubmitterNotes) && _changes.OldNotesStatus != _changes.NewNotesStatus)
                {
                    AddChangedField(data, "notes_status", "Status realizacji uwag", _changes.OldNotesStatus, _changes.NewNotesStatus);
                }
            }

            // Wiadomość administratora - dodaj tylko jeśli któraś wartość nie jest pusta
            if (!string.IsNullOrEmpty(_changes.OldAdminMessage) || !string.IsNullOrEmpty(_changes.NewAdminMessage))
            {
                AddChangedField(data, "admin_message", "Wiadomość od administratora", _changes.OldAdminMessage, _changes.NewAdminMessage);
            }

            // Harmonogram - dodaj tylko jeśli się zmienił
            if (_changes.OldSchedule != _changes.NewSchedule)
            {
                AddChangedField(data, "schedule", "Harmonogram", _changes.OldSchedule, _changes.NewSchedule);
            }

            return data;
        }

        /// <summary>
        /// Dodaje pole do danych tylko jeśli wartość się zmieniła, pogrubiając zmienione wartości
        /// </summary>
        private void AddChangedField(Dictionary<string, string> data, string fieldName, string label, string? oldValue, string? newValue)
        {
            oldValue = oldValue ?? "";
            newValue = newValue ?? "";

            // Jeśli wartości są różne, pogrub je
            if (oldValue != newValue)
            {
                data[$"old_{fieldName}"] = $"<b>{label}: {oldValue}</b>";
                data[$"new_{fieldName}"] = $"<b>{label}: {newValue}</b>";
            }
            else
            {
                // Jeśli się nie zmieniły, dodaj bez pogrubienia
                data[$"old_{fieldName}"] = $"{label}: {oldValue}";
                data[$"new_{fieldName}"] = $"{label}: {newValue}";
            }
        }
    }

    /// <summary>
    /// Klasa przechowująca informacje o zmianach w zgłoszeniu
    /// </summary>
    public class DataChanges
    {
        public string? OldName { get; set; }
        public string? OldAddress { get; set; }
        public string? OldEmail { get; set; }
        public string? OldSubmitterNotes { get; set; }
        public string? OldNotesStatus { get; set; }
        public string? OldAdminMessage { get; set; }
        public string? OldSchedule { get; set; }

        public string? NewName { get; set; }
        public string? NewAddress { get; set; }
        public string? NewEmail { get; set; }
        public string? NewSubmitterNotes { get; set; }
        public string? NewNotesStatus { get; set; }
        public string? NewAdminMessage { get; set; }
        public string? NewSchedule { get; set; }

        public bool HasChanges()
        {
            return OldName != NewName ||
                   OldAddress != NewAddress ||
                   OldEmail != NewEmail ||
                   OldSubmitterNotes != NewSubmitterNotes ||
                   OldNotesStatus != NewNotesStatus ||
                   OldAdminMessage != NewAdminMessage ||
                   OldSchedule != NewSchedule;
        }
    }
}
