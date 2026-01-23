using System.ComponentModel.DataAnnotations;
using SOK.Domain.Enums;

namespace SOK.Web.ViewModels.Settings
{
    public class CreateUserVM
    {
        [Required(ErrorMessage = "Nazwa wyświetlana jest wymagana.")]
        [MaxLength(64, ErrorMessage = "Nazwa wyświetlana może mieć maksymalnie 64 znaki.")]
        [Display(Name = "Nazwa wyświetlana")]
        public string DisplayName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Login jest wymagany.")]
        [MaxLength(64, ErrorMessage = "Login może mieć maksymalnie 64 znaki.")]
        [Display(Name = "Login")]
        public string UserName { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "Podaj poprawny adres email.")]
        [Display(Name = "Email")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Hasło jest wymagane.")]
        [MinLength(6, ErrorMessage = "Hasło musi mieć co najmniej 6 znaków.")]
        [Display(Name = "Hasło")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Potwierdź hasło.")]
        [Compare("Password", ErrorMessage = "Hasła muszą być takie same.")]
        [Display(Name = "Potwierdź hasło")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Display(Name = "Role")]
        public List<Role> SelectedRoles { get; set; } = new List<Role>();
    }
}
