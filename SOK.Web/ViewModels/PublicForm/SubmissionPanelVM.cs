using SOK.Domain.Enums;
using SOK.Web.ViewModels.ParishManagement;

namespace SOK.Web.ViewModels.PublicForm
{
    public class SubmissionPanelVM
    {
        public SubmissionInfoVM Submission { get; set; } = new();
        public VisitInfoVM Visit { get; set; } = new();
        public PlanScheduleInfoVM PlanSchedule { get; set; } = new();
        public ParishVM Parish { get; set; } = new();
    }

    public class SubmissionInfoVM
    {
        public string SubmitterName { get; set; } = string.Empty;
        public string SubmitterSurname { get; set; } = string.Empty;
        public string? SubmitterEmail { get; set; }
        public string? SubmitterPhone { get; set; }
        public string Address { get; set; } = string.Empty;
        public string? SubmitterNotes { get; set; }
        public string? AdminMessage { get; set; }
        public NotesFulfillmentStatus? NotesStatus { get; set; }
        public SubmitMethod SubmitMethod { get; set; }
        public DateTime SubmitTime { get; set; }
        public string UniqueId { get; set; } = string.Empty;
    }

    public class VisitInfoVM
    {
        public VisitStatus Status { get; set; }
        public int? OrdinalNumber { get; set; }
        public DateOnly? PlannedDate { get; set; }
        public bool DateVisible { get; set; } = true;
        public TimeOnly? EstimatedTime { get; set; }
        public TimeOnly? EstimatedTimeEnd { get; set; }
        public bool TimeVisible { get; set; } = false;
        public bool IsDynamicTimeRange { get; set; } = false;
    }

    public class PlanScheduleInfoVM
    {
        public string PlanName { get; set; } = string.Empty;
        public string? ScheduleName { get; set; }
        public string? ScheduleShortName { get; set; }
    }
}
