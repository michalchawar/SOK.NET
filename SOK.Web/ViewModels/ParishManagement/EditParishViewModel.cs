using System.ComponentModel.DataAnnotations;

namespace SOK.Web.ViewModels.ParishManagement
{
    /// <summary>
    /// Model widoku edycji parafii.
    /// </summary>
    public class EditParishViewModel
    {
        /// <summary>
        /// Identyfikator parafii.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Unikalny identyfikator parafii (UID).
        /// </summary>
        public string UniqueId { get; set; } = string.Empty;

        /// <summary>
        /// Nazwa parafii.
        /// </summary>
        [Required(ErrorMessage = "Nazwa parafii jest wymagana.")]
        [StringLength(200, ErrorMessage = "Nazwa parafii nie może być dłuższa niż 200 znaków.")]
        [Display(Name = "Nazwa parafii")]
        public string ParishName { get; set; } = string.Empty;
    }
}
