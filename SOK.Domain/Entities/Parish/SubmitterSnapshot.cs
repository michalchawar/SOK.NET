using System.ComponentModel.DataAnnotations;

namespace SOK.Domain.Entities.Parish
{
    /// <summary>
    /// Reprezentuje archiwalny stan danych osoby zgłaszającej (Submitter) w danym momencie.
    /// Pozwala śledzić historię zmian danych kontaktowych zgłaszającego.
    /// </summary>
    public class SubmitterSnapshot
    {
        /// <summary>
        /// Unikalny identyfikator snapshotu (klucz główny).
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Publiczny unikalny identyfikator zgłaszającego w momencie utworzenia snapshotu.
        /// W większości przypadków wszystkie snapshoty dla danego zgłaszającego będą miały ten sam UniqueId.
        /// </summary>
        public Guid UniqueId { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Imię osoby zgłaszającej w momencie utworzenia snapshotu.
        /// </summary>
        [MaxLength(64)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Nazwisko osoby zgłaszającej w momencie utworzenia snapshotu.
        /// </summary>
        [MaxLength(64)]
        public string Surname { get; set; } = string.Empty;

        /// <summary>
        /// Adres e-mail osoby zgłaszającej w momencie utworzenia snapshotu (opcjonalny).
        /// </summary>
        [MaxLength(256)]
        public string? Email { get; set; } = null;

        /// <summary>
        /// Numer telefonu osoby zgłaszającej w momencie utworzenia snapshotu (opcjonalny).
        /// </summary>
        [MaxLength(15)]
        public string? Phone { get; set; } = null;

        /// <summary>
        /// Data i godzina utworzenia snapshotu.
        /// </summary>
        public DateTime ChangeTime { get; private set; }

        /// <summary>
        /// Identyfikator użytkownika, który wprowadził zmianę, nadpisując dane z tego snapshotu.
        /// </summary>
        public int? ChangeAuthorId { get; set; }

        /// <summary>
        /// Użytkownik, który wprowadził zmianę, nadpisując dane z tego snapshotu (relacja opcjonalna).
        /// </summary>
        public ParishMember? ChangeAuthor { get; set; } = default!;
    }
}