using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SOK.Domain.Entities.Parish;
using SOK.Domain.Enums;

namespace SOK.Application.Common.DTO
{
    public class UserDto
    {
        public string CentralId { get; set; } = string.Empty;
        public int ParishId { get; set; }

        public string DisplayName { get; set; } = string.Empty;
        public string? UserName { get; set; } = string.Empty;
        public string? Email { get; set; } = string.Empty;
        public List<Role> Roles { get; set; } = new List<Role>();

        public ICollection<Agenda> AssignedAgendas { get; set; } = new List<Agenda>();
        public ICollection<Plan> AssignedPlans { get; set; } = new List<Plan>();
        public ICollection<FormSubmission> EnteredSubmissions { get; set; } = new List<FormSubmission>();
    }
}