using System.ComponentModel.DataAnnotations;

namespace SOK.Application.Common.DTO.Agenda
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

        public bool IsOfficial { get; set; }
        
        public bool ShowHours { get; set; }

        /// <summary>
        /// Liczba minut na jedną wizytę (jednostka czasowa).
        /// Jeśli null, używana jest wartość z księdza lub domyślna.
        /// </summary>
        public int? MinutesPerVisit { get; set; }
    }

    /// <summary>
    /// DTO reprezentujące agendę z podstawowymi informacjami.
    /// </summary>
    public class AgendaDto
    {
        public int Id { get; set; }
        public Guid UniqueId { get; set; }
        public string AccessToken { get; set; } = string.Empty;
        public int DayId { get; set; }
        public int PlanId { get; set; }
        public DateOnly Date { get; set; }
        public TimeOnly StartHour { get; set; }
        public TimeOnly EndHour { get; set; }
        public TimeOnly? StartHourOverride { get; set; }
        public TimeOnly? EndHourOverride { get; set; }
        public float? GatheredFunds { get; set; }
        public bool HideVisits { get; set; }
        public bool ShowHours { get; set; }
        public bool IsOfficial { get; set; }
        public int VisitsCount { get; set; }
        public string? AssignedPriestName { get; set; }

        public ParishMemberSimpleDto? Priest { get; set; }
        public List<ParishMemberSimpleDto> Ministers { get; set; } = new();

        /// <summary>
        /// Liczba minut na jedną wizytę (jednostka czasowa) dla tej agendy.
        /// </summary>
        public int? MinutesPerVisit { get; set; }
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
