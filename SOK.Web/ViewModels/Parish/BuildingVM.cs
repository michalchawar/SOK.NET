using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using SOK.Application.Common.Helpers;
using SOK.Domain.Entities.Parish;

namespace SOK.Web.ViewModels.Parish
{
    public class BuildingVM
    {
        [Required(ErrorMessage = "Podaj oznaczenie budynku.")]
        [RegularExpression(RegExpressions.BuildingNumberPattern, ErrorMessage = "Podaj poprawne oznaczenie budynku (numer i ewentualna litera).")]
        [Display(Name = "Numer (i litera) budynku")]
        public string Signage { get; set; } = string.Empty;

        [Display(Name = "Widoczny publicznie")]
        public bool IsVisible { get; set; } = true;


        [Display(Name = "Liczba pięter (-1 jeśli brak danych)")]
        [Range(-1, 100, ErrorMessage = "Liczba pięter musi być z zakresu od -1 do 100.")]
        public int FloorCount { get; set; } = -1;

        [Display(Name = "Liczba mieszkań (-1 jeśli brak danych)")]
        [Range(-1, 400, ErrorMessage = "Liczba mieszkań musi być z zakresu od -1 do 400.")]
        public int ApartmentCount { get; set; } = -1;
        
        [Display(Name = "Numer ostatniego mieszkania (-1 jeśli brak danych)")]
        [Range(-1, 299, ErrorMessage = "Numer mieszkania musi być z zakresu od -1 do 299.")]
        public int HighestApartmentNumber { get; set; } = -1;

        [Display(Name = "Posiada windę")]
        public bool HasElevator { get; set; } = false;

        
        [Required(ErrorMessage = "Wybierz ulicę.")]
        [Display(Name = "ID ulicy")]
        [HiddenInput(DisplayValue = false)]
        public int StreetId { get; set; }

        [ValidateNever]
        [Display(Name = "Ulica")]
        public string StreetName { get; set; } = string.Empty;
    }
}