using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SOK.Web.ViewModels.Parish
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
        [Display(Name = "Domyślny harmonogram")]
        public bool IsDefault { get; set; } = false;
    }
}