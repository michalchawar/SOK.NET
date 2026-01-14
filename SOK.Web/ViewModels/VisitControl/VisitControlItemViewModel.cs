using SOK.Domain.Enums;

namespace SOK.Web.ViewModels.VisitControl
{
    /// <summary>
    /// Reprezentacja pojedynczej wizyty w widoku przeprowadzania kolędy.
    /// </summary>
    public class VisitControlItemViewModel
    {
        public int VisitId { get; set; }
        public int SubmissionId { get; set; }
        public VisitStatus Status { get; set; }
        public int? PeopleCount { get; set; }
        public int OrdinalNumber { get; set; }
        
        // Dane adresowe (z cache w Address)
        public string? StreetName { get; set; }
        public string? BuildingNumber { get; set; }
        public string? BuildingLetter { get; set; }
        public int? ApartmentNumber { get; set; }
        public string? ApartmentLetter { get; set; }
        
        // Dodatkowe informacje
        public int? DeclaredPeopleCount { get; set; }
        public string? AdminNotes { get; set; }
        public int BuildingId { get; set; }
        public int ScheduleId { get; set; }
        public TimeOnly? EstimatedTime { get; set; }
    }
}
