using System.ComponentModel.DataAnnotations;

namespace SOK.Web.ViewModels.ParishManagement
{
    /// <summary>
    /// Model widoku tworzenia nowej parafii.
    /// </summary>
    public class CreateParishViewModel
    {
        /// <summary>
        /// Nazwa parafii.
        /// </summary>
        [Required(ErrorMessage = "Nazwa parafii jest wymagana.")]
        [StringLength(200, ErrorMessage = "Nazwa parafii nie może być dłuższa niż 200 znaków.")]
        [Display(Name = "Nazwa parafii")]
        public string ParishName { get; set; } = string.Empty;

        /// <summary>
        /// Czy utworzyć automatycznie administratora parafii.
        /// </summary>
        [Display(Name = "Utwórz administratora")]
        public bool CreateAdmin { get; set; } = true;

        /// <summary>
        /// Czy załadować przykładowe dane do parafii.
        /// </summary>
        [Display(Name = "Załaduj przykładowe dane")]
        public bool SeedExampleData { get; set; } = false;
    }
}
