using SOK.Domain.Entities.Parish;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SOK.Application.Common.DTO.Parish
{
    public class ParishDto
    {
        public string UniqueId { get; set; } = default!;
        public string FullName { get; set; } = default!;
        public string DioceseName { get; set; } = default!;
        public SOK.Domain.Entities.Parish.Address Address { get; set; } = default!;
    }
}
