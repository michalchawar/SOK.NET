namespace SOK.Application.Common.DTO
{
    /// <summary>
    /// DTO dla przypisywania budynków do dnia.
    /// </summary>
    public record AssignBuildingsDto(
        int DayId,
        int ScheduleId,
        List<int> BuildingIds,
        int? AgendaId = null
    );

    /// <summary>
    /// DTO dla odpisywania budynku od dnia.
    /// </summary>
    public record UnassignBuildingDto(
        int DayId,
        int BuildingId,
        int ScheduleId
    );

    /// <summary>
    /// Tryb zakresu dla kreatora przypisań.
    /// </summary>
    public enum RangeMode
    {
        /// <summary>
        /// Cała ulica.
        /// </summary>
        WholeStreet = 0,

        /// <summary>
        /// Zakres numerów (od-do).
        /// </summary>
        NumberRange = 1,

        /// <summary>
        /// Inteligentne przypisanie z kierunkiem.
        /// </summary>
        IntelligentAssignment = 2
    }

    /// <summary>
    /// Filtr parzystości numerów.
    /// </summary>
    [Flags]
    public enum ParityFilter
    {
        /// <summary>
        /// Bez filtra (wszystkie).
        /// </summary>
        None = 0,

        /// <summary>
        /// Tylko numery parzyste.
        /// </summary>
        Even = 1,

        /// <summary>
        /// Tylko numery nieparzyste.
        /// </summary>
        Odd = 2,

        /// <summary>
        /// Wszystkie numery (parzyste i nieparzyste).
        /// </summary>
        Both = Even | Odd
    }

    /// <summary>
    /// Kierunek przypisywania.
    /// </summary>
    public enum AssignmentDirection
    {
        /// <summary>
        /// Rosnąco (numery w górę).
        /// </summary>
        Ascending = 0,

        /// <summary>
        /// Malejąco (numery w dół).
        /// </summary>
        Descending = 1
    }

    /// <summary>
    /// DTO dla przypisywania budynków zakresem.
    /// </summary>
    public record AssignRangeDto(
        int DayId,
        int ScheduleId,
        int StreetId,
        RangeMode Mode,
        int? RangeFrom,
        int? RangeTo,
        int? StartNumber,
        AssignmentDirection? Direction,
        ParityFilter? ParityFilter,
        int? MaxSubmissions,
        int? AgendaId = null
    );

    /// <summary>
    /// DTO informacji o budynku z danymi przypisania.
    /// </summary>
    public class BuildingWithAssignmentInfoDto
    {
        public int BuildingId { get; set; }
        public int StreetId { get; set; }
        public string StreetName { get; set; } = string.Empty;
        public string BuildingNumber { get; set; } = string.Empty;
        public int ScheduleId { get; set; }
        public string ScheduleName { get; set; } = string.Empty;
        public int SubmissionsTotal { get; set; }
        public int SubmissionsUnassigned { get; set; }
        public int SubmissionsAssignedHere { get; set; }
        public bool IsAssignedToThisDay { get; set; }
        public bool IsAssignedToOtherDay { get; set; }
        public List<DateOnly> AssignedDayDates { get; set; } = new();
    }

    /// <summary>
    /// DTO podsumowania ulicy.
    /// </summary>
    public class StreetSummaryDto
    {
        public int StreetId { get; set; }
        public string StreetName { get; set; } = string.Empty;
        public int TotalBuildings { get; set; }
        public int AssignedBuildings { get; set; }
        public int TotalSubmissions { get; set; }
        public int AssignedSubmissions { get; set; }
        public double AssignmentPercentage => TotalBuildings > 0 
            ? (double)AssignedBuildings / TotalBuildings * 100 
            : 0;
    }
}
