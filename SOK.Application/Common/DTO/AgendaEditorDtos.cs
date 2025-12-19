using System.ComponentModel.DataAnnotations;

namespace SOK.Application.Common.DTO
{
    /// <summary>
    /// DTO reprezentujące wizytę w edytorze agendy.
    /// </summary>
    public class AgendaVisitDto
    {
        public int Id { get; set; }
        public int SubmissionId { get; set; }
        public int? OrdinalNumber { get; set; }
        public int BuildingId { get; set; }
        public string BuildingNumber { get; set; } = string.Empty;
        public int StreetId { get; set; }
        public string StreetTypeAbbrev { get; set; } = string.Empty;
        public string StreetName { get; set; } = string.Empty;
        public string SubmitterName { get; set; } = string.Empty;
        public string? ApartmentNumber { get; set; }
        public int? FloorNumber { get; set; }
        public string? SubmitterNotes { get; set; }
        public int? ScheduleId { get; set; }
        public string? ScheduleName { get; set; } = string.Empty;
        public string? ScheduleShortName { get; set; } = string.Empty;
        public string? ScheduleColor { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO reprezentujące bramę z listą zgłoszeń.
    /// </summary>
    public class BuildingWithSubmissionsDto
    {
        public int BuildingId { get; set; }
        public string BuildingNumber { get; set; } = string.Empty;
        public int StreetId { get; set; }
        public string StreetTypeAbbrev { get; set; } = string.Empty;
        public string StreetName { get; set; } = string.Empty;
        public List<SubmissionForAgendaDto> Submissions { get; set; } = new();
        public bool IsRecommended { get; set; }
        public int ScheduleId { get; set; }
        public string ScheduleName { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO reprezentujące zgłoszenie dostępne do dodania do agendy.
    /// </summary>
    public class SubmissionForAgendaDto
    {
        public int Id { get; set; }
        public int SubmissionId { get; set; }
        public int BuildingId { get; set; }
        public string BuildingNumber { get; set; } = string.Empty;
        public string StreetTypeAbbrev { get; set; } = string.Empty;
        public string StreetName { get; set; } = string.Empty;
        public string SubmitterName { get; set; } = string.Empty;
        public string? ApartmentNumber { get; set; }
        public int? FloorNumber { get; set; }
        public string? SubmitterNotes { get; set; }
        public int NotesStatus { get; set; }
        public string? AdminNotes { get; set; }
        public string? AdminMessage { get; set; }
        public int? ScheduleId { get; set; }
        public string? ScheduleName { get; set; }
        public bool IsAssigned { get; set; }
        public int? AssignedAgendaId { get; set; }
        public DateTime? AssignedDate { get; set; }
        public TimeOnly? AssignedStartHour { get; set; }
        public TimeOnly? AssignedEndHour { get; set; }
    }

    /// <summary>
    /// DTO do przypisywania wizyt do agendy.
    /// </summary>
    public class AssignVisitsToAgendaDto
    {
        [Required]
        public int AgendaId { get; set; }

        [Required]
        public List<int> SubmissionIds { get; set; } = new();

        public bool SendEmails { get; set; } = false;
    }

    /// <summary>
    /// DTO do aktualizacji kolejności wizyt w agendzie.
    /// </summary>
    public class UpdateVisitsOrderDto
    {
        [Required]
        public int AgendaId { get; set; }

        [Required]
        public List<VisitOrderDto> Visits { get; set; } = new();
    }

    public class VisitOrderDto
    {
        public int VisitId { get; set; }
        public int OrdinalNumber { get; set; }
    }

    /// <summary>
    /// DTO do usuwania wizyt z agendy.
    /// </summary>
    public class RemoveVisitsFromAgendaDto
    {
        [Required]
        public int AgendaId { get; set; }

        [Required]
        public List<int> VisitIds { get; set; } = new();
    }

    /// <summary>
    /// DTO do przepisywania wizyt między agendami.
    /// </summary>
    public class ReassignVisitsToAgendaDto
    {
        [Required]
        public int TargetAgendaId { get; set; }

        [Required]
        public List<int> SubmissionIds { get; set; } = new();
    }
}
