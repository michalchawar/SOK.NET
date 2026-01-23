using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using SOK.Application.Common.Helpers;
using SOK.Web.ViewModels.Plan;
using SOK.Web.ViewModels.ParishManagement;
using System.ComponentModel.DataAnnotations;

namespace SOK.Web.ViewModels.PublicForm
{
    public class NewSubmissionWebFormVM
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

        [Display(Name = "Chcę otrzymywać powiadomienia e-mail")]
        public bool WantsEmailNotification { get; set; } = false;

        [EmailAddress(ErrorMessage = "Podaj poprawny adres e-mail.")]
        [MinLength(5, ErrorMessage = "Adres e-mail musi mieć conajmniej 5 znaków.")]
        [MaxLength(200, ErrorMessage = "Adres e-mail może mieć maksymalnie 200 znaków.")]
        [RegularExpression(RegExpressions.EmailPattern, ErrorMessage = "Podaj poprawny adres e-mail.")]
        [Display(Name = "Adres e-mail")]
        public string? Email { get; set; } = null;


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

        [Display(Name = "Mam dodatkowe uwagi")]
        public bool HasAdditionalNotes { get; set; } = false;

        [MaxLength(500, ErrorMessage = "Wiadomość nie może przekraczać 500 znaków.")]
        [Display(Name = "Dodatkowe uwagi (opcjonalnie)")]
        public string? SubmitterNotes { get; set; } = null;

        [Required(ErrorMessage = "Musisz wyrazić zgodę na przetwarzanie danych osobowych.")]
        [Display(Name = "Wyrażam zgodę na przetwarzanie danych osobowych")]
        public bool GdprConsent { get; set; } = false;

        [ValidateNever]
        public ScheduleVM Schedule { get; set; } = default!;

        [ValidateNever]
        public ParishVM Parish { get; set; } = default!;

        [ValidateNever]
        public IEnumerable<SelectListItem> StreetList { get; set; } = default!;

        [ValidateNever]
        public IDictionary<int, IEnumerable<SelectListItem>> BuildingList { get; set; } = default!;
    }
}
