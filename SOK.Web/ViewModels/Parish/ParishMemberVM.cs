using System.ComponentModel.DataAnnotations;
using SOK.Domain.Enums;

namespace SOK.Web.ViewModels.Parish
{
    public class ParishMemberVM
    {
        public int? Id { get; set; }

        [Required]
        [MaxLength(64)]
        [Display(Name = "Wyświetlana nazwa")]
        public string DisplayName { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Przypisany do planu")]
        public bool IsActive { get; set; } = false;
    }
}