using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using SOK.Application.Common.Helpers;

namespace SOK.Web.ViewModels.Plan
{
    public class ScheduleVM
    {
        public int? Id { get; set; }

        [Required]
        [MaxLength(128)]
        [Display(Name = "Nazwa harmonogramu")]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(24)]
        [Display(Name = "Skrót")]
        public string ShortName { get; set; } = string.Empty;

        [Required]
        [MaxLength(7)]
        [Display(Name = "Kolor")]
        [RegularExpression(RegExpressions.RgbColorPattern, ErrorMessage = "Podaj poprawny kolor w formacie RGB (np. #RRGGBB).")]
        public string Color { get; set; } = "#bec5d1";

        [Required]
        [Display(Name = "Domyślny harmonogram")]
        public bool IsDefault { get; set; } = false;
    }
}