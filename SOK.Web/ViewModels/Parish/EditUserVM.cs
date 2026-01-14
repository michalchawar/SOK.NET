using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using SOK.Domain.Entities.Parish;
using SOK.Domain.Enums;

namespace SOK.Web.ViewModels.Parish
{
    public class EditUserVM
    {
        [Required]
        [ValidateNever]
        public string CentralId { get; set; } = string.Empty;

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

        [Display(Name = "Role")]
        public List<Role> SelectedRoles { get; set; } = new List<Role>();

        [Display(Name = "Przypisane plany")]
        public List<int> AssignedPlanIds { get; set; } = new List<int>();

        // For displaying available plans
        public List<Plan> AvailablePlans { get; set; } = new List<Plan>();
    }
}
