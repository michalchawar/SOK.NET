using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using SOK.Domain.Entities.Parish;
using System.ComponentModel.DataAnnotations;

namespace SOK.Web.ViewModels.Parish
{
    public class PlanVM
    {
        [Required(ErrorMessage = "Podaj nazwę planu.")]
        [MinLength(4, ErrorMessage = "Nazwa musi mieć conajmniej 4 znaki.")]
        [MaxLength(100, ErrorMessage = "Nazwa może mieć maksymalnie 100 znaków.")]
        [Display(Name = "Nazwa planu")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "Harmonogramy w planie")]
        [MinLength(1, ErrorMessage = "Dodaj co najmniej jeden harmonogram.")]
        public List<ScheduleVM> Schedules { get; set; } =
        [
            new ScheduleVM { Name = "Kolęda w terminie", ShortName = "T", IsDefault = true },
            new ScheduleVM { Name = "Kolęda dodatkowa", ShortName = "D", IsDefault = false }
        ];

        [ValidateNever]
        public IEnumerable<ParishMemberVM> Priests { get; set; } = new List<ParishMemberVM>();
    }
}
