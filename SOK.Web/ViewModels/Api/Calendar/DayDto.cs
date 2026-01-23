using System.ComponentModel.DataAnnotations;

namespace SOK.Web.ViewModels.Api.Calendar
{
    public class DayDto
    {
        public int? Id { get; set; }

        [Required]
        public DateOnly Date { get; set; }

        [Required]
        public TimeOnly StartHour { get; set; }

        [Required]
        public TimeOnly EndHour { get; set; }
    }
}
