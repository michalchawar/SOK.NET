namespace SOK.Web.ViewModels.Plan
{
    /// <summary>
    /// ViewModel dla pojedynczego planu w tabeli listy planów
    /// </summary>
    public class PlanListItemVM
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime CreationTime { get; set; }
        public string? AuthorName { get; set; }
        public int SubmissionsCount { get; set; }
        public int DaysCount { get; set; }
        public bool IsActive { get; set; }
    }
}
