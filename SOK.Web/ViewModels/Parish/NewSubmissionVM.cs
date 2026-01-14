using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using SOK.Application.Common.Helpers;
using SOK.Domain.Entities.Parish;
using SOK.Domain.Enums;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace SOK.Web.ViewModels.Parish
{
    public class NewSubmissionVM
    {
        [Required(ErrorMessage = "Podaj swoje imię.")]
        [MinLength(2, ErrorMessage = "Imię musi mieć conajmniej 2 znaki.")]
        [MaxLength(50, ErrorMessage = "Imię może mieć maksymalnie 50 znaków.")]
        [Display(Name = "Imię")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Podaj swoje nazwisko.")]
        [MinLength(2, ErrorMessage = "Nazwisko musi mieć conajmniej 2 znaki.")]
        [MaxLength(50, ErrorMessage = "Nazwisko może mieć maksymalnie 50 znaków.")]
        [Display(Name = "Nazwisko")]
        public string Surname { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "Podaj poprawny adres e-mail.")]
        [MinLength(5, ErrorMessage = "Adres e-mail musi mieć conajmniej 5 znaków.")]
        [MaxLength(200, ErrorMessage = "Adres e-mail może mieć maksymalnie 200 znaków.")]
        [Display(Name = "Adres e-mail")]
        public string? Email { get; set; } = null;

        [Phone(ErrorMessage = "Podaj poprawny numer telefonu.")]
        [MinLength(5, ErrorMessage = "Numer telefonu musi mieć conajmniej 5 znaków.")]
        [MaxLength(20, ErrorMessage = "Numer telefonu może mieć maksymalnie 20 znaków.")]
        [Display(Name = "Numer telefonu")]
        public string? Phone { get; set; } = null;


        [Required(ErrorMessage = "Wybierz swoją bramę.")]
        [Display(Name = "Brama")]
        public int BuildingId { get; set; } = default!;

        [Required(ErrorMessage = "Wybierz ulicę, aby zobaczyć dostępne bramy.")]
        [Display(Name = "Ulica")]
        public int StreetId { get; set; } = default!;


        [Required(ErrorMessage = "Podaj numer swojego mieszkania.")]
        [MaxLength(10, ErrorMessage = "Numer mieszkania może mieć maksymalnie 10 znaków.")]
        [RegularExpression(RegExpressions.ApartmentNumberPattern, ErrorMessage = "Podaj poprawny adres mieszkania.")]
        [Display(Name = "Numer mieszkania")]
        public string Apartment { get; set; } = string.Empty;

        [MaxLength(500, ErrorMessage = "Wiadomość nie może przekraczać 500 znaków.")]
        [Display(Name = "Wiadomość do administratora (opcjonalnie)")]
        public string? SubmitterNotes { get; set; } = null;

        [MaxLength(500, ErrorMessage = "Notatka nie może przekraczać 500 znaków.")]
        [Display(Name = "Notatka systemowa (wewnętrzna, opcjonalnie)")]
        public string? AdminNotes { get; set; } = null;

        [Display(Name = "Metoda otrzymania zgłoszenia")]
        [DefaultValue(SubmitMethod.PaperForm)]
        public SubmitMethod Method { get; set; } = SubmitMethod.PaperForm;

        [Display(Name = "Harmonogram")]
        public int ScheduleId { get; set; } = default!;

        [Display(Name = "Data przyjęcia")]
        public DateOnly SubmissionDate { get; set; } = DateOnly.FromDateTime(DateTime.Now);

        [Display(Name = "Wprowadź inną datę przyjęcia zgłoszenia")]
        [DefaultValue(false)]
        public bool UseCustomDate { get; set; } = false;

        [Display(Name = "Wyślij powiadomienie e-mail do zgłaszającego")]
        [DefaultValue(true)]
        public bool SendNotificationEmail { get; set; } = true;


        [ValidateNever]
        public IEnumerable<SelectListItem> StreetList { get; set; } = default!;

        [ValidateNever]
        public IEnumerable<SelectListItem> ScheduleList { get; set; } = default!;

        [ValidateNever]
        public IDictionary<int, IEnumerable<SelectListItem>> BuildingList { get; set; } = default!;
    }
}
