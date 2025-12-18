using SOK.Domain.Entities.Parish;

namespace SOK.Application.Common.DTO
{
    public class PlanActionRequestDto
    {
        public int? Id { get; set; } = null;
        public string Name { get; set; } = string.Empty;
        public IEnumerable<PlanScheduleDto> Schedules { get; set; } = new List<PlanScheduleDto>();
        public IEnumerable<PlanPriestDto> ActivePriests { get; set; } = new List<PlanPriestDto>();
        public ParishMember? Author { get; set; } = null;
    }

    public class PlanScheduleDto
    {
        public int? Id { get; set; } = null;
        public string Name { get; set; } = string.Empty;
        public string ShortName { get; set; } = string.Empty;
        public string Color { get; set; } = "#bec5d1";
        public bool IsDefault { get; set; } = false;
    }

    public class PlanPriestDto
    {
        public int? Id { get; set; } = null;
        public string DisplayName { get; set; } = string.Empty;
    }
}