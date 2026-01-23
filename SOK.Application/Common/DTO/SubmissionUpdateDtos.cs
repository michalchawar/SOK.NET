using SOK.Application.Common.Helpers;
using SOK.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace SOK.Application.Common.DTO
{
    /// <summary>
    /// DTO do selektywnej aktualizacji zgłoszenia (PATCH).
    /// Tylko pola które nie są null zostaną zaktualizowane.
    /// </summary>
    public class PatchSubmissionDto
    {
        // Dane zgłaszającego
        [MinLength(2, ErrorMessage = "Imię musi mieć conajmniej 2 znaki.")]
        [MaxLength(50, ErrorMessage = "Imię może mieć maksymalnie 50 znaków.")]
        public string? Name { get; set; }

        [MinLength(2, ErrorMessage = "Nazwisko musi mieć conajmniej 2 znaki.")]
        [MaxLength(50, ErrorMessage = "Nazwisko może mieć maksymalnie 50 znaków.")]
        public string? Surname { get; set; }

        [MaxLength(200, ErrorMessage = "Adres e-mail może mieć maksymalnie 200 znaków.")]
        [RegularExpression("(" + RegExpressions.EmailPattern + ")|(^$)", ErrorMessage = "Podaj poprawny adres e-mail.")]
        public string? Email { get; set; }

        [MaxLength(15, ErrorMessage = "Numer telefonu może mieć maksymalnie 15 znaków.")]
        [RegularExpression("(" + RegExpressions.PhoneNumberPattern + ")|(^$)", ErrorMessage = "Podaj poprawny numer telefonu.")]
        public string? Phone { get; set; }

        // Adres
        public int? BuildingId { get; set; }

        [MaxLength(6, ErrorMessage = "Numer mieszkania może mieć maksymalnie 6 znaków.")]
        [RegularExpression(RegExpressions.ApartmentNumberPattern, ErrorMessage = "Podaj poprawny numer mieszkania.")]
        public string? Apartment { get; set; }

        // Notatki
        [MaxLength(500, ErrorMessage = "Notatka zgłaszającego może mieć maksymalnie 500 znaków.")]
        public string? SubmitterNotes { get; set; }

        [MaxLength(500, ErrorMessage = "Wiadomość administratora może mieć maksymalnie 500 znaków.")]
        public string? AdminMessage { get; set; }

        [MaxLength(500, ErrorMessage = "Notatka administratora może mieć maksymalnie 500 znaków.")]
        public string? AdminNotes { get; set; }

        // Statusy i metadane
        public NotesFulfillmentStatus? NotesStatus { get; set; }
        
        public SubmitMethod? SubmitMethod { get; set; }

        // Harmonogram
        public int? ScheduleId { get; set; }

        // Wysyłanie maila o zmianach
        public bool SendDataChangeEmail { get; set; } = false;
    }
}
