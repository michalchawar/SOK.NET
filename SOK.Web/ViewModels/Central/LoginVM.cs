using System.ComponentModel.DataAnnotations;

namespace SOK.Web.ViewModels.Central
{
    public class LoginVM
    {
        [Required]
        [Display(Name = "Nazwa użytkownika")]
        public string Username { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Hasło")]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Zapamiętaj mnie")]
        public bool RememberMe { get; set; }
    }

}
