using System.ComponentModel.DataAnnotations;

namespace SOK.Web.ViewModels.Api.ParishMember
{
    /// <summary>
    /// DTO do ustawiania jednostki czasowej.
    /// </summary>
    public class SetMinutesPerVisitDto
    {
        [Required]
        [Range(3, 20)]
        public int MinutesPerVisit { get; set; }
    }
}
