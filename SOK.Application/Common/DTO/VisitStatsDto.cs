namespace SOK.Application.Common.DTO
{
    /// <summary>
    /// DTO zawierające statystyki kolędy dla planu
    /// </summary>
    public class VisitStatsDto
    {
        public string PlanName { get; set; } = string.Empty;

        // Ogólne statystyki
        public int PlannedSubmissions { get; set; }
        public int VisitedSubmissions { get; set; }
        public int UnplannedVisitedSubmissions { get; set; }
        public decimal RejectionPercentage { get; set; }
        public int TotalPeopleVisited { get; set; }
        public decimal AveragePeoplePerApartment { get; set; }

        // Średnio dziennie
        public decimal AveragePlannedSubmissionsPerDay { get; set; }
        public decimal AverageVisitedSubmissionsPerDay { get; set; }
        public decimal AveragePeoplePerDay { get; set; }

        public int TotalVisitDays { get; set; }

        // Tabele
        public List<DayStatsDto> DayStats { get; set; } = new();
        public List<SubmissionMethodStatsDto> SubmissionMethodStats { get; set; } = new();
    }

    /// <summary>
    /// Statystyki dla pojedynczego dnia kolędowego
    /// </summary>
    public class DayStatsDto
    {
        public DateOnly Date { get; set; }
        public string DayOfWeek { get; set; } = string.Empty;
        public int PlannedSubmissions { get; set; }
        public int AcceptedSubmissions { get; set; }
        public decimal RejectionPercentage { get; set; }
        public int PeopleVisited { get; set; }
        public decimal PeoplePerApartment { get; set; }
        public int PriestsCount { get; set; }
    }

    /// <summary>
    /// Statystyki zgłoszeń według metody i dnia
    /// </summary>
    public class SubmissionMethodStatsDto
    {
        public DateOnly Date { get; set; }
        public string DayOfWeek { get; set; } = string.Empty;
        public int WebFormSubmissions { get; set; }
        public int OtherMethodSubmissions { get; set; }
        public int DuringVisitSubmissions { get; set; }
    }
}
