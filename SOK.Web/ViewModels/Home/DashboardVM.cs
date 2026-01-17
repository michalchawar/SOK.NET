using SOK.Domain.Enums;

namespace SOK.Web.ViewModels.Home
{
    public class DashboardVM
    {
        public SubmissionsStatsVM SubmissionsStats { get; set; } = new();
        public List<DailySubmissionsVM> DailySubmissions { get; set; } = new();
        public UpcomingDayVM? UpcomingDay { get; set; }
        public List<CalendarDayVM> AllDays { get; set; } = new();
        public List<MinisterAgendaVM> MinisterAgendas { get; set; } = new();
    }

    public class SubmissionsStatsVM
    {
        public int TotalCount { get; set; }
        public int TotalLast24h { get; set; }
        public int WebFormCount { get; set; }
        public int WebFormLast24h { get; set; }
        public int OtherCount { get; set; }
        public int OtherLast24h { get; set; }
    }

    public class DailySubmissionsVM
    {
        public DateOnly Date { get; set; }
        public int WebFormCount { get; set; } = 0;
        public int OtherCount { get; set; } = 0;
        public int TotalCount => WebFormCount + OtherCount;
    }

    public class UpcomingDayVM
    {
        public int DayId { get; set; }
        public DateOnly Date { get; set; }
        public TimeOnly StartHour { get; set; }
        public TimeOnly EndHour { get; set; }
        public int TotalVisitsPlanned { get; set; }
        public List<AgendaCardVM> Agendas { get; set; } = new();
    }

    public class AgendaCardVM
    {
        public int AgendaId { get; set; }
        public string? PriestName { get; set; }
        public List<string> MinisterNames { get; set; } = new();
        public int VisitsCount { get; set; }
        public List<ScheduleStreetsCardVM> ScheduleStreets { get; set; } = new();
    }

    public class ScheduleStreetsCardVM
    {
        public string ScheduleName { get; set; } = string.Empty;
        public string ScheduleColor { get; set; } = string.Empty;
        public List<string> StreetNames { get; set; } = new();
    }

    public class CalendarDayVM
    {
        public int DayId { get; set; }
        public DateOnly Date { get; set; }
        public int VisitsPlanned { get; set; }
        public int VisitsCompleted { get; set; }
        public int AgendasCount { get; set; }
        public bool IsPast { get; set; }
        public bool IsUpcoming { get; set; }
    }

    public class MinisterAgendaVM
    {
        public int AgendaId { get; set; }
        public Guid AgendaUniqueId { get; set; }
        public string AccessToken { get; set; } = string.Empty;
        public DateOnly Date { get; set; }
        public TimeOnly StartHour { get; set; }
        public TimeOnly EndHour { get; set; }
        public string? PriestName { get; set; }
        public int VisitsCount { get; set; }
        public bool ShowHours { get; set; }
        public bool IsOfficial { get; set; }
        public bool IsPast { get; set; }
    }
}
