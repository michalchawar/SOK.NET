namespace SOK.Web.ViewModels.Calendar
{
    public class AssignedBuildingVM
    {
        public int BuildingId { get; set; }
        public int StreetId { get; set; }
        public string StreetName { get; set; } = string.Empty;
        public string BuildingNumber { get; set; } = string.Empty;
        public int SubmissionsTotal { get; set; }
        public int SubmissionsUnassigned { get; set; }
        public int SubmissionsAssignedHere { get; set; }
    }

    public class CompleteStreetVM
    {
        public int StreetId { get; set; }
        public string StreetName { get; set; } = string.Empty;
        public int BuildingsCount { get; set; }
        public int SubmissionsTotal { get; set; }
        public int SubmissionsAssignedHere { get; set; }
    }

    public class ScheduleWithBuildingsVM
    {
        public int ScheduleId { get; set; }
        public string ScheduleName { get; set; } = string.Empty;
        public string ScheduleColor { get; set; } = string.Empty;
        public List<AssignedBuildingVM> Buildings { get; set; } = new();
        public List<CompleteStreetVM> CompleteStreets { get; set; } = new();
        public int TotalBuildings => Buildings.Count + CompleteStreets.Sum(s => s.BuildingsCount);
        public int TotalSubmissions => Buildings.Sum(b => b.SubmissionsTotal) + CompleteStreets.Sum(s => s.SubmissionsTotal);
        public int TotalSubmissionsHere => Buildings.Sum(b => b.SubmissionsAssignedHere) + CompleteStreets.Sum(s => s.SubmissionsAssignedHere);
    }

    public class DayAssignmentsVM
    {
        public int DayId { get; set; }
        public List<ScheduleWithBuildingsVM> Schedules { get; set; } = new();
        public int TotalBuildingsAssigned => Schedules.Sum(s => s.TotalBuildings);
        public int TotalSubmissions => Schedules.Sum(s => s.TotalSubmissions);
        public int TotalSubmissionsHere => Schedules.Sum(s => s.TotalSubmissionsHere);
    }
}
