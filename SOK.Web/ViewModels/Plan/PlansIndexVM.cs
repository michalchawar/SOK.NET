namespace SOK.Web.ViewModels.Plan
{
    public class PlansIndexVM
    {
        public List<PlanListItemVM> Plans { get; set; } = new();
        public ActivePlanVM? ActivePlan { get; set; }
    }
}
