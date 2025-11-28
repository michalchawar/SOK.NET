using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace SOK.Web.ViewModels.Parish
{
    public class StreetVM
    {
        [Required(ErrorMessage = "Podaj nazwę ulicy.")]
        [MinLength(2, ErrorMessage = "Nazwa ulicy musi mieć conajmniej 2 znaki.")]
        [MaxLength(128, ErrorMessage = "Nazwa ulicy może mieć maksymalnie 128 znaków.")]
        [Display(Name = "Nazwa ulicy")]
        public string Name { get; set; } = string.Empty;

        
        [Required(ErrorMessage = "Wybierz rodzaj ulicy.")]
        [Display(Name = "Typ ulicy")]
        public int StreetSpecifierId { get; set; }
        
        [Required(ErrorMessage = "Wybierz miasto.")]
        [Display(Name = "Miasto")]
        public int CityId { get; set; }


        [ValidateNever]
        public IEnumerable<SelectListItem> StreetSpecifiers { get; set; } = default!;

        [ValidateNever]
        public IEnumerable<SelectListItem> Cities { get; set; } = default!;
    }
}