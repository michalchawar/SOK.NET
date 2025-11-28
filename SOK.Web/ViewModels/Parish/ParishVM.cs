using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SOK.Web.ViewModels.Parish
{
    public class ParishVM
    {
        public string Name { get; set; } = string.Empty;
        public string StreetAndBuilding { get; set; } = string.Empty;
        public string CityName { get; set; } = string.Empty;
        public string PostalCode { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string SecondaryPhone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }
}