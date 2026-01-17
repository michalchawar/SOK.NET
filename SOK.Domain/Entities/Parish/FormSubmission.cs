using SOK.Domain.Enums;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace SOK.Domain.Entities.Parish
{
    /// <summary>
    /// Reprezentuje pojedyncze zgłoszenie formularza przez użytkownika (anonimowego lub zalogowanego).
    /// Przechowuje dane osobowe, adresowe oraz metadane zgłoszenia w momencie jego wysłania.
    /// Stanowi archiwalny, oryginalny zapis danych, które mogły ulec zmianie.
    /// </summary>
    public class FormSubmission
    {
        /// <summary>
        /// Unikalny identyfikator zgłoszenia formularza (klucz główny).
        /// </summary>
        [Key]
        public int Id { get; set; }

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
        /// Dodatkowe uwagi zgłaszającego (opcjonalne).
        /// </summary>
        [MaxLength(512)]
        public string? SubmitterNotes { get; set; } = null;

        /// <summary>
        /// Nazwa harmonogramu, do którego zgłoszenie zostało przypisane.
        /// </summary>
        [MaxLength(64)]
        public string ScheduleName { get; set; } = string.Empty;

        /// <summary>
        /// Numer (i opcjonalnie litera) mieszkania podany w zgłoszeniu.
        /// </summary>
        [MaxLength(16)]
        public string Apartment { get; set; } = string.Empty;

        /// <summary>
        /// Numer (i opcjonalnie litera) budynku podana w zgłoszeniu.
        /// </summary>
        [MaxLength(16)]
        public string Building { get; set; } = string.Empty;

        /// <summary>
        /// Typ ulicy (np. Ulica, Aleja, Plac) w momencie składania zgłoszenia.
        /// </summary>
        [MaxLength(32)]
        public string StreetSpecifier { get; set; } = string.Empty;

        /// <summary>
        /// Nazwa ulicy podana w zgłoszeniu.
        /// </summary>
        [MaxLength(128)]
        public string Street { get; set; } = string.Empty;

        /// <summary>
        /// Nazwa miasta w momencie składania zgłoszenia.
        /// </summary>
        [MaxLength(128)]
        public string City { get; set; } = string.Empty;

        /// <summary>
        /// Metoda zgłoszenia (np. formularz papierowy, online, telefonicznie).
        /// </summary>
        [DefaultValue(SubmitMethod.NotRegistered)]
        public SubmitMethod Method { get; set; } = SubmitMethod.NotRegistered;

        /// <summary>
        /// Adres IP, z którego otrzymano zgłoszenie.
        /// Jeśli zgłoszenie zostało wprowadzone manualnie przez zalogowanego użytkownika,
        /// to jest to adres IP tego użytkownika.
        /// </summary>
        [MaxLength(64)]
        public string IP { get; set; } = string.Empty;

        /// <summary>
        /// Data i godzina otrzymania zgłoszenia.
        /// </summary>
        public DateTime SubmitTime { get; private set; }

        /// <summary>
        /// Identyfikator użytkownika, który utworzył zgłoszenie 
        /// (jeśli zostało wprowadzone manualnie).
        /// </summary>
        public int? AuthorId { get; set; }

        /// <summary>
        /// Użytkownik, który utworzył zgłoszenie (relacja opcjonalna).
        /// </summary>
        public ParishMember? Author { get; set; } = default!;

        /// <summary>
        /// Identyfikator powiązanego zgłoszenia głównego (Submission).
        /// </summary>
        public int SubmissionId { get; set; }

        /// <summary>
        /// Powiązane zgłoszenie główne (relacja nawigacyjna).
        /// </summary>
        public Submission Submission { get; set; } = default!;
    }
}