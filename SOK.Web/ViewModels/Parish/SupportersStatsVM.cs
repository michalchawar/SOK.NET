namespace SOK.Web.ViewModels.Parish
{
    public class SupportersStatsVM
    {
        public List<SupporterStatsItemVM> Supporters { get; set; } = new();
        public string PlanName { get; set; } = string.Empty;
    }

    public class SupporterStatsItemVM
    {
        public string DisplayName { get; set; } = string.Empty;
        public int VisitCount { get; set; }
    }
}
