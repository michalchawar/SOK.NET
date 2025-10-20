using SOK.Domain.Enums;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace SOK.Domain.Entities.Parish
{
    /// <summary>
    /// Reprezentuje archiwalny stan zg³oszenia (Submission) w danym momencie.
    /// Pozwala œledziæ historiê zmian danych zg³oszenia, w tym adresowych i statusowych.
    /// </summary>
    public class SubmissionSnapshot
    {
        /// <summary>
        /// Unikalny identyfikator snapshotu (klucz g³ówny).
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Publiczny unikalny identyfikator zg³oszenia w momencie utworzenia snapshotu.
        /// W wiêkszoœci przypadków wszystkie snapshoty dla danego zg³oszenia bêd¹ mia³y ten sam UniqueId.
        /// </summary>
        public Guid UniqueId { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Token dostêpu do zg³oszenia w momencie utworzenia snapshotu.
        /// </summary>
        [MaxLength(64)]
        public string AccessToken { get; set; } = default!;

        /// <summary>
        /// Uwagi zg³aszaj¹cego z momentu utworzenia snapshotu (opcjonalne).
        /// </summary>
        [MaxLength(512)]
        public string? SubmitterNotes { get; set; }

        /// <summary>
        /// Wiadomoœæ administracyjna z momentu utworzenia snapshotu (opcjonalna).
        /// </summary>
        [MaxLength(512)]
        public string? AdminMessage { get; set; }

        /// <summary>
        /// Systemowe notatki administratora z momentu utworzenia snapshotu (opcjonalne).
        /// </summary>
        [MaxLength(512)]
        public string? AdminNotes { get; set; }

        /// <summary>
        /// Status realizacji notatek w momencie utworzenia snapshotu.
        /// </summary>
        [DefaultValue(NotesFulfillmentStatus.NA)]
        public NotesFulfillmentStatus NotesStatus { get; set; }

        /// <summary>
        /// Numer (i opcjonalnie litera) mieszkania z momentu utworzenia snapshotu.
        /// </summary>
        [MaxLength(16)]
        public string Apartment { get; set; } = default!;

        /// <summary>
        /// Numer (i opcjonalnie litera) budynku z momentu utworzenia snapshotu.
        /// </summary>
        [MaxLength(16)]
        public string Building { get; set; } = default!;

        /// <summary>
        /// Typ ulicy z momentu utworzenia snapshotu.
        /// </summary>
        [MaxLength(32)]
        public string StreetSpecifier { get; set; } = default!;

        /// <summary>
        /// Nazwa ulicy z momentu utworzenia snapshotu.
        /// </summary>
        [MaxLength(128)]
        public string Street { get; set; } = default!;

        /// <summary>
        /// Nazwa miasta z momentu utworzenia snapshotu.
        /// </summary>
        [MaxLength(128)]
        public string City { get; set; } = default!;

        /// <summary>
        /// Nazwa diecezji z momentu utworzenia snapshotu.
        /// </summary>
        [MaxLength(128)]
        public string Diocese { get; set; } = default!;

        /// <summary>
        /// Data i godzina utworzenia snapshotu.
        /// </summary>
        public DateTime ChangeTime { get; private set; }

        /// <summary>
        /// Identyfikator u¿ytkownika, który wprowadzi³ zmianê, nadpisuj¹c dane z tego snapshotu.
        /// </summary>
        public int? ChangeAuthorId { get; set; } = default!;

        /// <summary>
        /// U¿ytkownik, który wprowadzi³ zmianê, nadpisuj¹c dane z tego snapshotu (relacja nawigacyjna).
        /// </summary>
        public User? ChangeAuthor { get; set; } = default!;

        /// <summary>
        /// Identyfikator zg³oszenia, którego dotyczy snapshot.
        /// </summary>
        public int SubmissionId { get; set; } = default!;

        /// <summary>
        /// Zg³oszenie, którego dotyczy snapshot (relacja nawigacyjna).
        /// </summary>
        public Submission Submission { get; set; } = default!;
    }
}