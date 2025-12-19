using SOK.Domain.Entities.Parish;
using SOK.Domain.Enums;

namespace SOK.Application.Common.DTO
{
    public class VisitDto
    {
        public int Id { get; set; }

        public int? OrdinalNumber { get; set; }
        public VisitStatus Status { get; set; }
        public TimeOnly? StartHour { get; set; }
        public TimeOnly? EndHour { get; set; }

        public DateOnly? PlannedDate { get; set; }
        public TimeOnly? EstimatedTime { get; set; }
        public bool DateVisible { get; set; }
        public bool TimeVisible { get; set; }
        public int? AgendaId { get; set; } 

        public ScheduleDto Schedule { get; set; }

        public VisitDto(Visit visit)
        {
            Id = visit.Id;
            OrdinalNumber = visit.OrdinalNumber;
            Status = visit.Status;
            PlannedDate = visit.Agenda != null ? visit.Agenda.Day.Date : null;
            EstimatedTime = null;
            StartHour = visit.Agenda?.StartHourOverride ?? visit.Agenda?.Day.StartHour;
            EndHour = visit.Agenda?.EndHourOverride ?? visit.Agenda?.Day.EndHour;
            DateVisible = !visit.Agenda?.HideVisits ?? true;
            TimeVisible = visit.Agenda?.ShowHours ?? false;
            AgendaId = visit.Agenda?.Id;

            Schedule = visit.Status != VisitStatus.Withdrawn ? new ScheduleDto(visit.Schedule!) : null!;
        }
    }
}
