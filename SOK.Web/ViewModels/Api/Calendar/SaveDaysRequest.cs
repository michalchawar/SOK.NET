using System.ComponentModel.DataAnnotations;

namespace SOK.Web.ViewModels.Api.Calendar
{
    /// <summary>
    /// DTO dla zapisu dni.
    /// </summary>
    public class SaveDaysRequest
    {
        [Required]
        public int PlanId { get; set; }

        [Required]
        public DateTime VisitsStartDate { get; set; }

        [Required]
        public DateTime VisitsEndDate { get; set; }

        [Required]
        public List<DayDto> Days { get; set; } = new();
    }
}
