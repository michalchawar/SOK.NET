using SOK.Domain.Entities.Parish;
using SOK.Domain.Enums;

namespace SOK.Application.Common.DTO.Plan
{
    public class PlanDto
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public PlanDto(SOK.Domain.Entities.Parish.Plan plan)
        {
            Id = plan.Id;
            Name = plan.Name;
        }
    }
}
