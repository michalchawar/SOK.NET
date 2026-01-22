using Microsoft.AspNetCore.Identity;
using SOK.Application.Common.DTO;
using SOK.Application.Common.Interface;
using SOK.Application.Services.Interface;
using SOK.Domain.Entities.Central;
using SOK.Domain.Entities.Parish;
using SOK.Domain.Enums;
using System.Linq.Expressions;

namespace SOK.Application.Services.Implementation
{
    /// <inheritdoc />
    public class PlanService : IPlanService
    {
        private readonly IUnitOfWorkParish _uow;
        private readonly IUnitOfWorkCentral _uowCentral;

        public PlanService(IUnitOfWorkParish uow, IUnitOfWorkCentral uowCentral)
        {
            _uow = uow;
            _uowCentral = uowCentral;
        }

        /// <inheritdoc />
        public async Task<Plan?> GetPlanAsync(int id)
        {
            return await _uow.Plan.GetAsync(p => p.Id == id, includeProperties: "Schedules");
        }

        /// <inheritdoc />
        public async Task<List<Plan>> GetPlansPaginatedAsync(
            Expression<Func<Plan, bool>>? filter = null,
            int page = 1,
            int pageSize = 1)
        {
            if (pageSize < 1) throw new ArgumentException("Page size must be positive.");
            if (page < 1) throw new ArgumentException("Page must be positive.");

            List<Plan> result = [.. await _uow.Plan.GetPaginatedAsync(
                    filter,
                    pageSize: pageSize,
                    page: page,
                    author: true,
                    submissions: true,
                    days: true)];

            return result;
        }

        /// <inheritdoc />
        public async Task CreatePlanAsync(Plan plan)
        {
            _uow.Plan.Add(plan);
            await _uow.SaveAsync();
        }

        /// <inheritdoc />
        public async Task<bool> DeletePlanAsync(int id)
        {
            try
            {
                Plan? plan = await _uow.Plan.GetAsync(p => p.Id == id);
                if (plan != null)
                {
                    _uow.Plan.Remove(plan);
                    await _uow.SaveAsync();
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <inheritdoc />
        public async Task UpdatePlanAsync(Plan plan)
        {
            _uow.Plan.Update(plan);
            await _uow.SaveAsync();
        }

        /// <inheritdoc />
        public async Task SetActivePlanAsync(Plan plan)
        {
            Plan? entity = await _uow.Plan.GetAsync(p => p.Id == plan.Id, includeProperties: "Schedules");

            if (entity is null)
            {
                throw new ArgumentException("Plan does not exist in the database.");
            }

            await _uow.ParishInfo.SetValueAsync("ActivePlanId", plan.Id.ToString());
            await _uow.SaveAsync();
        }

        /// <inheritdoc />
        public async Task<Plan?> GetActivePlanAsync()
        {
            string? planIdStr = await _uow.ParishInfo.GetValueAsync("ActivePlanId");

            if (string.IsNullOrEmpty(planIdStr))
            {
                return null;
            }

            int planId = int.Parse(planIdStr);
            return await _uow.Plan.GetAsync(
                filter: p => p.Id == planId, 
                includeProperties: "Schedules");
        }

        /// <inheritdoc />
        public async Task ClearActivePlanAsync()
        {
            await _uow.ParishInfo.ClearValueAsync("ActivePlanId");
            await _uow.SaveAsync();
        }

        /// <inheritdoc />
        public async Task ToggleSubmissionGatheringAsync(Plan plan, bool enabled)
        {
            await _uow.ParishInfo.SetMetadataAsync(plan, "IsSubmissionGatheringEnabled", enabled.ToString());
            await _uow.SaveAsync();
        }

        /// <inheritdoc />
        public async Task<bool> IsSubmissionGatheringEnabledAsync(Plan plan)
        {
            string? value = await _uow.ParishInfo.GetMetadataAsync(plan, "IsSubmissionGatheringEnabled");
            if (string.IsNullOrEmpty(value))
                return false;

            if (bool.TryParse(value, out bool result))
                return result;

            return false;
        }

        /// <inheritdoc />
        public async Task CreatePlanAsync(PlanActionRequestDto planDto)
        {
            PlanScheduleDto? defaultScheduleDto;
            try
            {
                defaultScheduleDto = planDto.Schedules.SingleOrDefault(s => s.IsDefault);
            }
            catch (InvalidOperationException)
            {
                throw new ArgumentException("There are more than one default schedules set.");
            }

            await using var transaction = await _uow.BeginTransactionAsync();

            Plan plan = new()
            {
                Name = planDto.Name,
                Author = planDto.Author,
            };
            _uow.Plan.Add(plan);
            await _uow.SaveAsync();

            var schedules = planDto.Schedules.Select(s => new Schedule()
            {
                Name = s.Name,
                ShortName = s.ShortName,
                Color = s.Color,
                Plan = plan
            }).ToList();
            foreach (Schedule schedule in schedules)
            {
                _uow.Schedule.Add(schedule);
            }
            await _uow.SaveAsync();

            Schedule? defaultSchedule = defaultScheduleDto is null ?
                null :
                schedules.FirstOrDefault(s => s.Name == defaultScheduleDto.Name && s.ShortName == defaultScheduleDto.ShortName);

            plan.DefaultSchedule = defaultSchedule;
            await _uow.SaveAsync();

            var existingPriestIds = planDto.ActivePriests.Where(pm => pm.Id is not null).Select(pm => pm.Id ?? -1);
            var existingPriests = await _uow.ParishMember.GetAllAsync(pm => existingPriestIds.Contains(pm.Id));

            await using var transactionCentral = await _uowCentral.BeginTransactionAsync();

            foreach (PlanPriestDto newPriest in planDto.ActivePriests)
            {
                ParishMember? priest = existingPriests.FirstOrDefault(pm => pm.Id == newPriest.Id);
                if (priest is null)
                {
                    // Utwórz nowego użytkownika z generycznymi danymi
                    priest = await _uow.ParishMember.CreateMemberWithUserAccountAsync(newPriest.DisplayName, [Role.Priest]);
                    
                    // Zapisz, aby wygenerować Id przed dodaniem do kolekcji
                    await _uow.SaveAsync();
                }

                // Dodaj użytkownika do planu
                if (priest is not null)
                    plan.ActivePriests.Add(priest);
            }

            await _uow.SaveAsync();

            // Włącz przyjmowanie zgłoszeń, domyślnie
            await _uow.ParishInfo.SetMetadataAsync(plan, "IsSubmissionGatheringEnabled", true.ToString());
            await _uow.SaveAsync();

            await transaction.CommitAsync();
            await transactionCentral.CommitAsync();
        }

        /// <inheritdoc />
        public async Task UpdatePlanAsync(PlanActionRequestDto planDto)
        {
            PlanScheduleDto? defaultScheduleDto;
            try
            {
                defaultScheduleDto = planDto.Schedules.SingleOrDefault(s => s.IsDefault);
            }
            catch (InvalidOperationException)
            {
                throw new ArgumentException("There are more than one default schedules set.");
            }

            await using var transaction = await _uow.BeginTransactionAsync();

            // Pobierz obiekt planu z relacją ActivePriests
            Plan? plan = await _uow.Plan.GetAsync(
                filter: p => p.Id == planDto.Id, 
                includeProperties: "ActivePriests",
                tracked: true);
            if (planDto.Id is null || plan is null)
                throw new ArgumentException("Cannot update plan: plan with set id not found.");

            // Zaktualizuj nazwę
            plan.Name = planDto.Name;
            plan.DefaultScheduleId = null;

            // Określ zbiór z ID-kami istniejących planów i pobierz je na jego podstawie
            var existingScheduleIds = planDto.Schedules.Where(s => s.Id is not null).Select(s => s.Id ?? -1);
            var existingSchedules = await _uow.Schedule.GetAllAsync(
                filter: s => existingScheduleIds.Contains(s.Id), 
                includeProperties: "Plan",
                tracked: true);

            // Dla każdego DTO harmonogramu określ, czy mamy je już w bazie (na podstawie pobranych) i odpowiednio zaktualizuj lub dodaj
            foreach (PlanScheduleDto newSchedule in planDto.Schedules)
            {
                Schedule? schedule = existingSchedules.FirstOrDefault(s => s.Id == newSchedule.Id);
                if (schedule is not null)
                {
                    schedule.Name = newSchedule.Name;
                    schedule.ShortName = newSchedule.ShortName;
                    schedule.Color = newSchedule.Color;
                    schedule.Plan = plan;
                }
                else
                {
                    schedule = new()
                    {
                        Name = newSchedule.Name,
                        ShortName = newSchedule.ShortName,
                        Color = newSchedule.Color,
                        Plan = plan
                    };
                    _uow.Schedule.Add(schedule);
                }

                if (newSchedule.IsDefault)
                    plan.DefaultScheduleId = schedule.Id;
            }

            // Usuń harmonogramy, które nie występują w DTO
            var includedScheduleIds = planDto.Schedules.Where(s => s.Id is not null).Select(s => s.Id ?? -1);
            var schedulesToRemove = await _uow.Schedule.GetAllAsync(s => s.PlanId == plan.Id && !includedScheduleIds.Contains(s.Id));
            foreach (Schedule scheduleToRemove in schedulesToRemove)
            {
                _uow.Schedule.Remove(scheduleToRemove);
            }

            await _uow.SaveAsync();

            // Przygotuj listę księży do aktualizacji
            var existingPriestIds = planDto.ActivePriests.Where(pm => pm.Id is not null).Select(pm => pm.Id ?? -1);
            var existingPriests = await _uow.ParishMember.GetAllAsync(
                filter: pm => existingPriestIds.Contains(pm.Id));

            await using var transactionCentral = await _uowCentral.BeginTransactionAsync();

            var priestsToKeep = new List<ParishMember>();

            foreach (PlanPriestDto newPriest in planDto.ActivePriests)
            {
                ParishMember? priest = existingPriests.FirstOrDefault(pm => pm.Id == newPriest.Id);
                if (priest is null)
                {
                    // Utwórz nowego użytkownika z generycznymi danymi
                    priest = await _uow.ParishMember.CreateMemberWithUserAccountAsync(newPriest.DisplayName, [Role.Priest]);
                    
                    // Zapisz, aby wygenerować Id przed dodaniem do kolekcji
                    await _uow.SaveAsync();
                }

                if (priest is not null)
                    priestsToKeep.Add(priest);
            }

            // Usuń księży, którzy nie są w nowej liście
            var priestsToRemove = plan.ActivePriests.Where(p => !priestsToKeep.Any(pk => pk.Id == p.Id)).ToList();
            foreach (var priest in priestsToRemove)
            {
                plan.ActivePriests.Remove(priest);
            }

            // Dodaj nowych księży (tylko tych, którzy jeszcze nie są w kolekcji)
            foreach (var priest in priestsToKeep)
            {
                if (!plan.ActivePriests.Any(p => p.Id == priest.Id))
                {
                    plan.ActivePriests.Add(priest);
                }
            }

            await _uow.SaveAsync();
            await transaction.CommitAsync();
            await transactionCentral.CommitAsync();
        }

        /// <inheritdoc />
        public async Task<DateTime?> GetDateTimeMetadataAsync(Plan plan, string metadataKey)
        {
            string? value = await _uow.ParishInfo.GetMetadataAsync(plan, metadataKey);
            if (string.IsNullOrEmpty(value))
                return null;

            if (DateTime.TryParse(value, out DateTime result))
                return result;

            return null;
        }

        /// <inheritdoc />
        public async Task SetDateTimeMetadataAsync(Plan plan, string metadataKey, DateTime value)
        {
            await _uow.ParishInfo.SetMetadataAsync(plan, metadataKey, value.ToString("O"));
            await _uow.SaveAsync();
        }

        /// <inheritdoc />
        public async Task<TimeOnly?> GetTimeMetadataAsync(Plan plan, string metadataKey)
        {
            string? value = await _uow.ParishInfo.GetMetadataAsync(plan, metadataKey);
            if (string.IsNullOrEmpty(value))
                return null;

            if (TimeOnly.TryParse(value, out TimeOnly result))
                return result;

            return null;
        }

        /// <inheritdoc />
        public async Task SetTimeMetadataAsync(Plan plan, string metadataKey, TimeOnly value)
        {
            await _uow.ParishInfo.SetMetadataAsync(plan, metadataKey, value.ToString("c"));
            await _uow.SaveAsync();
        }

        /// <inheritdoc />
        public async Task DeleteMetadataAsync(Plan plan, string metadataKey)
        {
            await _uow.ParishInfo.DeleteMetadataAsync(plan, metadataKey);
            await _uow.SaveAsync();
        }

        /// <inheritdoc />
        public async Task<List<Day>> GetDaysForPlanAsync(int planId)
        {
            var days = await _uow.Day.GetAllAsync(
                filter: d => d.PlanId == planId,
                includeProperties: "Agendas.Visits,Agendas.AssignedMembers",
                orderBy: d => d.Date
            );
            return days.OrderBy(d => d.Date).ToList();
        }

        /// <inheritdoc />
        public async Task<Day?> GetDayAsync(int dayId)
        {
            return await _uow.Day.GetAsync(d => d.Id == dayId, includeProperties: "BuildingsAssigned");
        }

        /// <inheritdoc />
        public async Task ManageDaysAsync(int planId, List<Day> days, DateTime visitsStartDate, DateTime visitsEndDate)
        {
            await using var transaction = await _uow.BeginTransactionAsync();

            // Pobierz plan
            Plan? plan = await _uow.Plan.GetAsync(p => p.Id == planId);
            if (plan == null)
                throw new ArgumentException("Plan o podanym ID nie istnieje.");

            // Zapisz metadane dat
            await SetDateTimeMetadataAsync(plan, Common.Helpers.PlanMetadataKeys.VisitsStartDate, visitsStartDate);
            await SetDateTimeMetadataAsync(plan, Common.Helpers.PlanMetadataKeys.VisitsEndDate, visitsEndDate);

            // Pobierz istniejące dni dla tego planu
            var existingDays = await _uow.Day.GetAllAsync(d => d.PlanId == planId);
            var existingDaysDict = existingDays.ToDictionary(d => d.Date);

            // Przygotuj słownik nowych dni
            var newDaysDict = days.ToDictionary(d => d.Date);

            // Usuń dni, które nie są w nowej liście
            foreach (var existingDay in existingDays)
            {
                if (!newDaysDict.ContainsKey(existingDay.Date))
                {
                    _uow.Day.Remove(existingDay);
                }
            }

            // Aktualizuj istniejące lub dodaj nowe
            foreach (var day in days)
            {
                if (existingDaysDict.TryGetValue(day.Date, out var existingDay))
                {
                    // Aktualizuj istniejący
                    existingDay.StartHour = day.StartHour;
                    existingDay.EndHour = day.EndHour;
                    _uow.Day.Update(existingDay);
                }
                else
                {
                    // Dodaj nowy
                    day.PlanId = planId;
                    _uow.Day.Add(day);
                }
            }

            await _uow.SaveAsync();
            await transaction.CommitAsync();
        }

        /// <inheritdoc />
        public async Task<VisitStatsDto?> GetVisitStatsAsync(int planId)
        {
            var plan = await _uow.Plan.GetAsync(
                p => p.Id == planId, 
                includeProperties: "Submissions.Visit,Submissions.FormSubmission");
            if (plan == null)
                return null;

            // Pobierz dni z pełnymi danymi
            var days = await _uow.Day.GetAllAsync(
                filter: d => d.PlanId == planId,
                includeProperties: "Agendas.Visits.Submission.FormSubmission",
                orderBy: d => d.Date
            );

            var allSubmissions = plan.Submissions.ToList();
            var allVisits = allSubmissions.Select(s => s.Visit).ToList();
            
            // Ogólne statystyki
            var plannedSubmissions = allVisits.Count(v => v.Status == VisitStatus.Planned || v.Status == VisitStatus.Visited || v.Status == VisitStatus.Rejected);
            var visitedSubmissions = allVisits.Count(v => v.Status == VisitStatus.Visited);
            var unplannedVisitedSubmissions = allVisits.Count(v => v.Status == VisitStatus.Visited && v.Submission.FormSubmission?.Method == SubmitMethod.DuringVisits);
            var rejectedSubmissions = allVisits.Count(v => v.Status == VisitStatus.Rejected);
            var totalPeopleVisited = allVisits.Where(v => v.Status == VisitStatus.Visited && v.PeopleCount.HasValue)
                .Sum(v => v.PeopleCount ?? 0);
            var visitedWithPeopleCount = allVisits.Count(v => v.Status == VisitStatus.Visited && v.PeopleCount.HasValue && v.PeopleCount > 0);

            var rejectionPercentage = plannedSubmissions > 0 
                ? Math.Round((decimal)rejectedSubmissions / plannedSubmissions * 100, 2) 
                : 0;
            var averagePeoplePerApartment = visitedWithPeopleCount > 0 
                ? Math.Round((decimal)totalPeopleVisited / visitedWithPeopleCount, 2) 
                : 0;

            // Średnio dziennie (tylko dni kolędowe, które mają zaplanowane wizyty)
            var daysWithVisits = days.Count(d => d.Agendas.Any(a => a.Visits.Any() && a.IsOfficial));
            var averagePlannedPerDay = daysWithVisits > 0 
                ? Math.Round((decimal)plannedSubmissions / daysWithVisits, 2) 
                : 0;
            var averageVisitedPerDay = daysWithVisits > 0 
                ? Math.Round((decimal)visitedSubmissions / daysWithVisits, 2) 
                : 0;
            var averagePeoplePerDay = daysWithVisits > 0 
                ? Math.Round((decimal)totalPeopleVisited / daysWithVisits, 2) 
                : 0;

            // Statystyki dzienne - pierwsza tabela
            var dayStats = days.Select(day =>
            {
                var dayVisits = day.Agendas.Where(a => a.IsOfficial).SelectMany(a => a.Visits).ToList();
                var dayPlanned = dayVisits.Count(v => v.Status == VisitStatus.Planned || v.Status == VisitStatus.Visited || v.Status == VisitStatus.Rejected);
                var dayAccepted = dayVisits.Count(v => v.Status == VisitStatus.Visited);
                var dayRejected = dayVisits.Count(v => v.Status == VisitStatus.Rejected);
                var dayPeople = dayVisits.Where(v => v.Status == VisitStatus.Visited && v.PeopleCount.HasValue)
                    .Sum(v => v.PeopleCount ?? 0);
                var dayVisitedWithPeople = dayVisits.Count(v => v.Status == VisitStatus.Visited && v.PeopleCount.HasValue && v.PeopleCount > 0);
                var dayPriestsCount = day.Agendas.Count(a => a.IsOfficial);

                return new DayStatsDto
                {
                    Date = day.Date,
                    DayOfWeek = day.Date.DayOfWeek.ToString(),
                    PlannedSubmissions = dayPlanned,
                    AcceptedSubmissions = dayAccepted,
                    RejectionPercentage = dayPlanned > 0 ? Math.Round((decimal)dayRejected / dayPlanned * 100, 2) : 0,
                    PeopleVisited = dayPeople,
                    PeoplePerApartment = dayVisitedWithPeople > 0 ? Math.Round((decimal)dayPeople / dayVisitedWithPeople, 2) : 0,
                    PriestsCount = dayPriestsCount
                };
            }).ToList();

            // Druga tabela - statystyki według metod zgłoszeń
            // Znajdź zakres dat od pierwszego zgłoszenia do ostatniego lub dni kolędowych
            var firstSubmissionDate = allSubmissions.Any() 
                ? DateOnly.FromDateTime(allSubmissions.Min(s => s.SubmitTime))
                : (days.Any() ? days.Min(d => d.Date) : DateOnly.FromDateTime(DateTime.Today));
            var lastSubmissionDate = allSubmissions.Any() 
                ? DateOnly.FromDateTime(allSubmissions.Max(s => s.SubmitTime))
                : (days.Any() ? days.Max(d => d.Date) : DateOnly.FromDateTime(DateTime.Today));
            var lastDayDate = days.Any() ? days.Max(d => d.Date) : DateOnly.FromDateTime(DateTime.Today);

            var startDate = firstSubmissionDate < (days.Any() ? days.Min(d => d.Date) : firstSubmissionDate) 
                ? firstSubmissionDate 
                : (days.Any() ? days.Min(d => d.Date) : firstSubmissionDate);
            var endDate = lastSubmissionDate > lastDayDate ? lastSubmissionDate : lastDayDate;

            var submissionMethodStats = new List<SubmissionMethodStatsDto>();
            for (var date = startDate; date <= endDate; date = date.AddDays(1))
            {
                var dateTime = date.ToDateTime(TimeOnly.MinValue);
                var daySubmissions = allSubmissions.Where(s => DateOnly.FromDateTime(s.SubmitTime) == date).ToList();

                var webFormCount = daySubmissions.Count(s => s.FormSubmission?.Method == SubmitMethod.WebForm);
                var otherMethodCount = daySubmissions.Count(s => s.FormSubmission?.Method != SubmitMethod.WebForm 
                    && s.FormSubmission?.Method != SubmitMethod.DuringVisits);
                var duringVisitCount = daySubmissions.Count(s => s.FormSubmission?.Method == SubmitMethod.DuringVisits);

                submissionMethodStats.Add(new SubmissionMethodStatsDto
                {
                    Date = date,
                    DayOfWeek = date.DayOfWeek.ToString(),
                    WebFormSubmissions = webFormCount,
                    OtherMethodSubmissions = otherMethodCount,
                    DuringVisitSubmissions = duringVisitCount
                });
            }

            return new VisitStatsDto
            {
                PlanName = plan.Name,
                PlannedSubmissions = plannedSubmissions,
                VisitedSubmissions = visitedSubmissions,
                UnplannedVisitedSubmissions = unplannedVisitedSubmissions,
                RejectionPercentage = rejectionPercentage,
                TotalPeopleVisited = totalPeopleVisited,
                AveragePeoplePerApartment = averagePeoplePerApartment,
                AveragePlannedSubmissionsPerDay = averagePlannedPerDay,
                AverageVisitedSubmissionsPerDay = averageVisitedPerDay,
                AveragePeoplePerDay = averagePeoplePerDay,
                TotalVisitDays = days.Count(),
                DayStats = dayStats,
                SubmissionMethodStats = submissionMethodStats
            };
        }
    }
}
