using System.ComponentModel.DataAnnotations;

namespace SOK.Application.Common.DTO
{
    /// <summary>
    /// DTO do tworzenia lub aktualizacji agendy.
    /// </summary>
    public class SaveAgendaDto
    {
        public int? Id { get; set; }

        [Required]
        public int DayId { get; set; }

        public TimeOnly? StartHourOverride { get; set; }

        public TimeOnly? EndHourOverride { get; set; }

        public int? PriestId { get; set; }

        public List<int> MinisterIds { get; set; } = new();

        public bool HideVisits { get; set; }
        
        public bool ShowHours { get; set; }
    }

    /// <summary>
    /// DTO reprezentujące agendę z podstawowymi informacjami.
    /// </summary>
    public class AgendaDto
    {
        public int Id { get; set; }
        public Guid UniqueId { get; set; }
        public int DayId { get; set; }
        public TimeOnly? StartHourOverride { get; set; }
        public TimeOnly? EndHourOverride { get; set; }
        public float? GatheredFunds { get; set; }
        public bool HideVisits { get; set; }
        public bool ShowHours { get; set; }
        public int VisitsCount { get; set; }

        public ParishMemberSimpleDto? Priest { get; set; }
        public List<ParishMemberSimpleDto> Ministers { get; set; } = new();
    }

    /// <summary>
    /// Uproszczony DTO reprezentujący członka parafii.
    /// </summary>
    public class ParishMemberSimpleDto
    {
        public int Id { get; set; }
        public string DisplayName { get; set; } = string.Empty;
    }
}
