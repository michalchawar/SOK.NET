using SOK.Domain.Enums;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace SOK.Domain.Entities.Parish
{
    /// <summary>
    /// Reprezentuje archiwalny stan zgłoszenia (Submission) w danym momencie.
    /// Pozwala śledzić historię zmian danych zgłoszenia, w tym adresowych i statusowych.
    /// </summary>
    public class SubmissionSnapshot
    {
        /// <summary>
        /// Unikalny identyfikator snapshotu (klucz główny).
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Publiczny unikalny identyfikator zgłoszenia w momencie utworzenia snapshotu.
        /// W większości przypadków wszystkie snapshoty dla danego zgłoszenia będą miały ten sam UniqueId.
        /// </summary>
        public Guid UniqueId { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Token dostępu do zgłoszenia w momencie utworzenia snapshotu.
        /// </summary>
        [MaxLength(64)]
        public string AccessToken { get; set; } = string.Empty;

        /// <summary>
        /// Uwagi zgłaszającego z momentu utworzenia snapshotu (opcjonalne).
        /// </summary>
        [MaxLength(512)]
        public string? SubmitterNotes { get; set; } = null;

        /// <summary>
        /// Wiadomość administracyjna z momentu utworzenia snapshotu (opcjonalna).
        /// </summary>
        [MaxLength(512)]
        public string? AdminMessage { get; set; } = null;

        /// <summary>
        /// Systemowe notatki administratora z momentu utworzenia snapshotu (opcjonalne).
        /// </summary>
        [MaxLength(512)]
        public string? AdminNotes { get; set; } = null;

        /// <summary>
        /// Status realizacji notatek w momencie utworzenia snapshotu.
        /// </summary>
        [DefaultValue(NotesFulfillmentStatus.NA)]
        public NotesFulfillmentStatus NotesStatus { get; set; } = NotesFulfillmentStatus.NA;

        /// <summary>
        /// Numer (i opcjonalnie litera) mieszkania z momentu utworzenia snapshotu.
        /// </summary>
        [MaxLength(16)]
        public string Apartment { get; set; } = string.Empty;

        /// <summary>
        /// Numer (i opcjonalnie litera) budynku z momentu utworzenia snapshotu.
        /// </summary>
        [MaxLength(16)]
        public string Building { get; set; } = string.Empty;

        /// <summary>
        /// Typ ulicy z momentu utworzenia snapshotu.
        /// </summary>
        [MaxLength(32)]
        public string StreetSpecifier { get; set; } = string.Empty;

        /// <summary>
        /// Nazwa ulicy z momentu utworzenia snapshotu.
        /// </summary>
        [MaxLength(128)]
        public string Street { get; set; } = string.Empty;

        /// <summary>
        /// Nazwa miasta z momentu utworzenia snapshotu.
        /// </summary>
        [MaxLength(128)]
        public string City { get; set; } = string.Empty;

        /// <summary>
        /// Data i godzina utworzenia snapshotu.
        /// </summary>
        public DateTime ChangeTime { get; private set; }

        /// <summary>
        /// Identyfikator użytkownika, który wprowadził zmianę, nadpisując dane z tego snapshotu.
        /// </summary>
        public int? ChangeAuthorId { get; set; }

        /// <summary>
        /// Użytkownik, który wprowadził zmianę, nadpisując dane z tego snapshotu (relacja nawigacyjna).
        /// </summary>
        public ParishMember? ChangeAuthor { get; set; } = default!;

        /// <summary>
        /// Identyfikator zgłoszenia, którego dotyczy snapshot.
        /// </summary>
        public int SubmissionId { get; set; }

        /// <summary>
        /// Zgłoszenie, którego dotyczy snapshot (relacja nawigacyjna).
        /// </summary>
        public Submission Submission { get; set; } = default!;
    }
}