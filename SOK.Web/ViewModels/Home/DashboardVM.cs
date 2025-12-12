namespace SOK.Web.ViewModels.Home
{
    public class DashboardVM
    {
        public SubmissionsStatsVM SubmissionsStats { get; set; } = new();
        public List<DailySubmissionsVM> DailySubmissions { get; set; } = new();
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
}
