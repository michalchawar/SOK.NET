using SOK.Domain.Enums;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace SOK.Domain.Entities.Parish
{
    /// <summary>
    /// Reprezentuje pojedyncze zg³oszenie formularza przez u¿ytkownika (anonimowego lub zalogowanego).
    /// Przechowuje dane osobowe, adresowe oraz metadane zg³oszenia w momencie jego wys³ania.
    /// Stanowi archiwalny, oryginalny zapis danych, które mog³y ulec zmianie.
    /// </summary>
    public class FormSubmission
    {
        /// <summary>
        /// Unikalny identyfikator zg³oszenia formularza (klucz g³ówny).
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Imiê osoby zg³aszaj¹cej.
        /// </summary>
        [MaxLength(64)]
        public string Name { get; set; } = default!;

        /// <summary>
        /// Nazwisko osoby zg³aszaj¹cej.
        /// </summary>
        [MaxLength(64)]
        public string Surname { get; set; } = default!;

        /// <summary>
        /// Adres e-mail osoby zg³aszaj¹cej (opcjonalny).
        /// </summary>
        [MaxLength(256)]
        public string? Email { get; set; }

        /// <summary>
        /// Numer telefonu osoby zg³aszaj¹cej (opcjonalny).
        /// </summary>
        [MaxLength(15)]
        public string? Phone { get; set; }

        /// <summary>
        /// Dodatkowe uwagi zg³aszaj¹cego (opcjonalne).
        /// </summary>
        [MaxLength(512)]
        public string? SubmitterNotes { get; set; }

        /// <summary>
        /// Nazwa harmonogramu, do którego zg³oszenie zosta³o przypisane.
        /// </summary>
        [MaxLength(64)]
        public string ScheduleName { get; set; } = default!;

        /// <summary>
        /// Numer (i opcjonalnie litera) mieszkania podany w zg³oszeniu.
        /// </summary>
        [MaxLength(16)]
        public string Apartment { get; set; } = default!;

        /// <summary>
        /// Numer (i opcjonalnie litera) budynku podana w zg³oszeniu.
        /// </summary>
        [MaxLength(16)]
        public string Building { get; set; } = default!;

        /// <summary>
        /// Typ ulicy (np. Ulica, Aleja, Plac) w momencie sk³adania zg³oszenia.
        /// </summary>
        [MaxLength(32)]
        public string StreetSpecifier { get; set; } = default!;

        /// <summary>
        /// Nazwa ulicy podana w zg³oszeniu.
        /// </summary>
        [MaxLength(128)]
        public string Street { get; set; } = default!;

        /// <summary>
        /// Nazwa miasta w momencie sk³adania zg³oszenia.
        /// </summary>
        [MaxLength(128)]
        public string City { get; set; } = default!;

        /// <summary>
        /// Metoda zg³oszenia (np. formularz papierowy, online, telefonicznie).
        /// </summary>
        [DefaultValue(SubmitMethod.NotRegistered)]
        public SubmitMethod Method { get; set; }

        /// <summary>
        /// Adres IP, z którego otrzymano zg³oszenie.
        /// Jeœli zg³oszenie zosta³o wprowadzone manualnie przez zalogowanego u¿ytkownika,
        /// to jest to adres IP tego u¿ytkownika.
        /// </summary>
        [MaxLength(64)]
        public string IP { get; set; } = default!;

        /// <summary>
        /// Data i godzina otrzymania zg³oszenia.
        /// </summary>
        public DateTime SubmitTime { get; private set; }

        /// <summary>
        /// Identyfikator u¿ytkownika, który utworzy³ zg³oszenie 
        /// (jeœli zosta³o wprowadzone manualnie).
        /// </summary>
        public int? AuthorId { get; set; } = default!;

        /// <summary>
        /// U¿ytkownik, który utworzy³ zg³oszenie (relacja opcjonalna).
        /// </summary>
        public User? Author { get; set; } = default!;

        /// <summary>
        /// Identyfikator powi¹zanego zg³oszenia g³ównego (Submission).
        /// </summary>
        public int SubmissionId { get; set; } = default!;

        /// <summary>
        /// Powi¹zane zg³oszenie g³ówne (relacja nawigacyjna).
        /// </summary>
        public Submission? Submission { get; set; } = default!;
    }
}