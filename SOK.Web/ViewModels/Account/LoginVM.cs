using System.ComponentModel.DataAnnotations;

namespace SOK.Web.ViewModels.Account
{
    public class LoginVM
    {
        [Required(ErrorMessage = "Podaj nazwę użytkownika")]
        [Display(Name = "Nazwa użytkownika")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Podaj hasło")]
        [DataType(DataType.Password)]
        [Display(Name = "Hasło")]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Zapamiętaj mnie")]
        public bool RememberMe { get; set; }
    }

}
