using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using SOK.Domain.Entities.Parish;

namespace SOK.Web.ViewModels.Settings
{
    public class BuildingSeriesVM
    {
        [Required(ErrorMessage = "Podaj numer początkowy.")]
        [Display(Name = "Numer pierwszego budynku")]
        [Range(1, 299, ErrorMessage = "Podaj poprawny numer budynku.")]
        public int From { get; set; }

        [Required(ErrorMessage = "Podaj numer końcowy.")]
        [Display(Name = "Numer ostatniego budynku")]
        [Range(1, 299, ErrorMessage = "Podaj poprawny numer budynku.")]
        public int To { get; set; }

        [Display(Name = "Posiadają windę")]
        public bool HasElevator { get; set; } = false;

        [Display(Name = "Tryb tworzenia")]
        [AllowedValues(0, 1, 2, ErrorMessage = "Nieprawidłowa wartość trybu tworzenia.")]
        [Description("0 - Wszystkie, 1 - Parzyste, 2 - Nieparzyste")]
        public int InsertMode { get; set; } = 0;

        
        [Required(ErrorMessage = "Wybierz ulicę.")]
        [Display(Name = "ID ulicy")]
        [HiddenInput(DisplayValue = false)]
        public int StreetId { get; set; }

        [ValidateNever]
        [Display(Name = "Ulica")]
        public string StreetName { get; set; } = string.Empty;
    }
}