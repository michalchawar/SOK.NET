using SOK.Domain.Entities.Parish;
using SOK.Domain.Enums;

namespace SOK.Application.Common.DTO
{
    public class PlanDto
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public PlanDto(Plan plan)
        {
            Id = plan.Id;
            Name = plan.Name;
        }
    }
}
