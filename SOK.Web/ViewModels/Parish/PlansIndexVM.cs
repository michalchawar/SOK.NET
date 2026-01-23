namespace SOK.Web.ViewModels.Parish
{
    public class PlansIndexVM
    {
        public List<PlanListItemVM> Plans { get; set; } = new();
        public ActivePlanVM? ActivePlan { get; set; }
    }
}
