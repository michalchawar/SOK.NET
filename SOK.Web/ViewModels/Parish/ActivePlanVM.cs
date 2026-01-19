namespace SOK.Web.ViewModels.Parish
{
    /// <summary>
    /// ViewModel dla aktywnego planu z pełnymi informacjami
    /// </summary>
    public class ActivePlanVM
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime CreationTime { get; set; }
        public List<ScheduleVM> Schedules { get; set; } = new();
        public int? DefaultScheduleId { get; set; }
        public int VisitsPlanned { get; set; }
        public int VisitsSucceeded { get; set; }
        public int VisitsRejected { get; set; }
        public decimal TotalFunds { get; set; }
        public int AgendasCount { get; set; }
        public int SupportersCount { get; set; }
        public bool IsPublicFormEnabled { get; set; }
    }
}
