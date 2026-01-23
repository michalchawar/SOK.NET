using SOK.Application.Common.DTO;

namespace SOK.Web.ViewModels.Parish
{
    public class VisitStatsVM
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
        public List<DayStatsItemVM> DayStats { get; set; } = new();
        public List<SubmissionMethodStatsItemVM> SubmissionMethodStats { get; set; } = new();

        public VisitStatsVM() { }

        public VisitStatsVM(VisitStatsDto dto)
        {
            PlanName = dto.PlanName;
            PlannedSubmissions = dto.PlannedSubmissions;
            VisitedSubmissions = dto.VisitedSubmissions;
            UnplannedVisitedSubmissions = dto.UnplannedVisitedSubmissions;
            RejectionPercentage = dto.RejectionPercentage;
            TotalPeopleVisited = dto.TotalPeopleVisited;
            AveragePeoplePerApartment = dto.AveragePeoplePerApartment;
            AveragePlannedSubmissionsPerDay = dto.AveragePlannedSubmissionsPerDay;
            AverageVisitedSubmissionsPerDay = dto.AverageVisitedSubmissionsPerDay;
            AveragePeoplePerDay = dto.AveragePeoplePerDay;
            TotalVisitDays = dto.TotalVisitDays;
            DayStats = dto.DayStats.Select(d => new DayStatsItemVM(d)).ToList();
            SubmissionMethodStats = dto.SubmissionMethodStats.Select(s => new SubmissionMethodStatsItemVM(s)).ToList();
        }
    }

    public class DayStatsItemVM
    {
        public DateOnly Date { get; set; }
        public string DayOfWeek { get; set; } = string.Empty;
        public int PlannedSubmissions { get; set; }
        public int AcceptedSubmissions { get; set; }
        public decimal RejectionPercentage { get; set; }
        public int PeopleVisited { get; set; }
        public decimal PeoplePerApartment { get; set; }
        public int PriestsCount { get; set; }

        public DayStatsItemVM() { }

        public DayStatsItemVM(DayStatsDto dto)
        {
            Date = dto.Date;
            DayOfWeek = dto.DayOfWeek;
            PlannedSubmissions = dto.PlannedSubmissions;
            AcceptedSubmissions = dto.AcceptedSubmissions;
            RejectionPercentage = dto.RejectionPercentage;
            PeopleVisited = dto.PeopleVisited;
            PeoplePerApartment = dto.PeoplePerApartment;
            PriestsCount = dto.PriestsCount;
        }
    }

    public class SubmissionMethodStatsItemVM
    {
        public DateOnly Date { get; set; }
        public string DayOfWeek { get; set; } = string.Empty;
        public int WebFormSubmissions { get; set; }
        public int OtherMethodSubmissions { get; set; }
        public int DuringVisitSubmissions { get; set; }

        public SubmissionMethodStatsItemVM() { }

        public SubmissionMethodStatsItemVM(SubmissionMethodStatsDto dto)
        {
            Date = dto.Date;
            DayOfWeek = dto.DayOfWeek;
            WebFormSubmissions = dto.WebFormSubmissions;
            OtherMethodSubmissions = dto.OtherMethodSubmissions;
            DuringVisitSubmissions = dto.DuringVisitSubmissions;
        }
    }
}
