using SOK.Application.Common.DTO;
using SOK.Domain.Enums;

namespace SOK.Web.ViewModels.VisitControl
{
    /// <summary>
    /// ViewModel dla widoku przeprowadzania wizyty kolędowej.
    /// </summary>
    public class VisitControlViewModel
    {
        public int AgendaId { get; set; }
        public Guid AgendaUniqueId { get; set; }
        public string AccessToken { get; set; } = string.Empty;
        public DateOnly Date { get; set; }
        public TimeOnly StartHour { get; set; }
        public TimeOnly EndHour { get; set; }
        public float? GatheredFunds { get; set; }
        public int? AssignedPriestId { get; set; }
        public string? AssignedPriestName { get; set; }
        
        public List<VisitControlItemViewModel> Visits { get; set; } = new();
        public List<ParishMemberSimpleDto> AvailablePriests { get; set; } = new();
        public List<BuildingSimpleDto> AvailableBuildings { get; set; } = new();
    }
}
