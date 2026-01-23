using SOK.Domain.Entities.Parish;
using SOK.Domain.Enums;

namespace SOK.Application.Common.DTO.Schedule
{
    public class ScheduleDto
    {
        public int Id { get; set; }

        public string Name { get; set; }
        public string ShortName { get; set; }
        public string Color { get; set; }

        public ScheduleDto(SOK.Domain.Entities.Parish.Schedule schedule)
        {
            Id = schedule.Id;
            Name = schedule.Name;
            ShortName = schedule.ShortName;
            Color = schedule.Color;
        }
    }
}
