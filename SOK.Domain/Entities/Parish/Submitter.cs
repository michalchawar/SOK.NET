using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;

namespace SOK.Domain.Entities.Parish
{
    /// <summary>
    /// Reprezentuje osobę zgłaszającą.
    /// Przechowuje podstawowe dane kontaktowe oraz powiązania ze zgłoszeniami i historią zmian.
    /// </summary>
    public class Submitter
    {
        /// <summary>
        /// Unikalny identyfikator osoby zgłaszającej (klucz główny).
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Publiczny unikalny identyfikator osoby zgłaszającej (GUID).
        /// </summary>
        public Guid UniqueId { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Imię osoby zgłaszającej.
        /// </summary>
        [MaxLength(64)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Nazwisko osoby zgłaszającej.
        /// </summary>
        [MaxLength(64)]
        public string Surname { get; set; } = string.Empty;

        /// <summary>
        /// Adres e-mail osoby zgłaszającej (opcjonalny).
        /// </summary>
        [MaxLength(256)]
        public string? Email { get; set; } = null;

        /// <summary>
        /// Numer telefonu osoby zgłaszającej (opcjonalny).
        /// </summary>
        [MaxLength(15)]
        public string? Phone { get; set; } = null;

        /// <summary>
        /// Lista zgłoszeń powiązanych z osobą zgłaszającą.
        /// </summary>
        public ICollection<Submission> Submissions { get; set; } = new List<Submission>();

        /// <summary>
        /// Historia zmian danych osoby zgłaszającej (snapshoty).
        /// </summary>
        public ICollection<SubmitterSnapshot> History { get; set; } = new List<SubmitterSnapshot>();


        /// <summary>
        /// Reprezentacja tekstowa zgłaszającego do celów filtrowania.
        /// </summary>
        /// <remarks>
        /// Łączny tekst do wyszukiwania (imię, nazwisko, e-mail, telefon w różnych kolejnościach),
        /// generowany automatycznie w bazie danych.
        /// </remarks>
        [MaxLength(1024)]
        public string FilterableString { get; private set; } = string.Empty;

        public bool IsEqual(Submitter other)
        {
            return this.Name == other.Name
                && this.Surname == other.Surname
                && this.Email == other.Email
                && this.Phone == other.Phone;
        }

        public static Expression<Func<Submitter, bool>> IsEqualExpression(Submitter other)
        {
            return s => s.Name.ToLower() == other.Name.ToLower()
                && s.Surname.ToLower() == other.Surname.ToLower()
                && (s.Email ?? string.Empty).ToLower() == (other.Email ?? string.Empty).ToLower()
                && (s.Phone ?? string.Empty).ToLower() == (other.Phone ?? string.Empty).ToLower();
        }
    }
}