namespace SOK.Web.ViewModels.Calendar
{
    public class AgendaItemVM
    {
        public int Id { get; set; }
        public string? PriestName { get; set; }
        public List<string> MinisterNames { get; set; } = new();
        public TimeOnly? StartHourOverride { get; set; }
        public TimeOnly? EndHourOverride { get; set; }
        public int VisitsCount { get; set; }
        public bool ShowsAssignment { get; set; }
        public bool ShowHours { get; set; }
        public bool IsOfficial { get; set; }
        public float GatheredFunds { get; set; }
    }

    public class DayAgendasVM
    {
        public int DayId { get; set; }
        public List<AgendaItemVM> Agendas { get; set; } = new();
    }
}
