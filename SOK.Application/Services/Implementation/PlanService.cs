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
        private readonly UserManager<User> _userManager;

        public PlanService(IUnitOfWorkParish uow, UserManager<User> userManager)
        {
            _uow = uow;
            _userManager = userManager;
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
            return await _uow.Plan.GetAsync(p => p.Id == planId);
        }

        /// <inheritdoc />
        public async Task ClearActivePlanAsync()
        {
            await _uow.ParishInfo.ClearValueAsync("ActivePlanId");
            await _uow.SaveAsync();
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
            
            foreach (PlanPriestDto newPriest in planDto.ActivePriests)
            {
                ParishMember? priest = existingPriests.FirstOrDefault(pm => pm.Id == newPriest.Id);
                if (priest is null)
                {
                    // Utwórz nowego użytkownika z generycznymi danymi
                    User centralUser = await _uow.ParishMember.GenerateUserAsync(newPriest.DisplayName);
                    priest = new()
                    {
                        DisplayName = newPriest.DisplayName,
                        CentralUserId = centralUser.Id,
                    };
                    var result = await _userManager.CreateAsync(centralUser);

                    if (result.Succeeded)
                    {
                        _uow.ParishMember.Add(priest);
                        await _userManager.AddToRoleAsync(centralUser, Role.Priest.ToString());
                    }
                    else
                    {
                        continue;
                    }
                }

                // Dodaj użytkownika do planu
                plan.ActivePriests.Add(priest);
            }

            await _uow.SaveAsync();
            await transaction.CommitAsync();
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

            // Pobierz obiekt planu
            Plan? plan = await _uow.Plan.GetAsync(p => p.Id == planDto.Id, includeProperties: "DefaultSchedule", tracked: true);
            if (planDto.Id is null || plan is null)
                throw new ArgumentException("Cannot update plan: plan with set id not found.");

            // Zaktualizuj nazwę
            plan.Name = planDto.Name;

            // Określ zbiór z ID-kami istniejących planów i pobierz je na jego podstawie
            var existingScheduleIds = planDto.Schedules.Where(s => s.Id is not null).Select(s => s.Id ?? -1);
            var existingSchedules = await _uow.Schedule.GetAllAsync(s => existingScheduleIds.Contains(s.Id), includeProperties: "Plan");

            // Dla każdego DTO harmonogramu określ, czy mamy je już w bazie (na podstawie pobranych) i odpowiednio zaktualizuj lub dodaj
            foreach (PlanScheduleDto newSchedule in planDto.Schedules)
            {
                Schedule? schedule = existingSchedules.FirstOrDefault(s => s.Id == newSchedule.Id);
                if (schedule is not null)
                {
                    schedule.Name = newSchedule.Name;
                    schedule.ShortName = newSchedule.ShortName;
                    schedule.Plan = plan;
                    _uow.Schedule.Update(schedule);
                }
                else
                {
                    schedule = new()
                    {
                        Name = newSchedule.Name,
                        ShortName = newSchedule.ShortName,
                        Plan = plan
                    };
                    _uow.Schedule.Add(schedule);
                }

                if (newSchedule.IsDefault)
                    plan.DefaultSchedule = schedule;
            }

            await _uow.SaveAsync();

            plan.ActivePriests.Clear();

            var existingPriestIds = planDto.ActivePriests.Where(pm => pm.Id is not null).Select(pm => pm.Id ?? -1);
            var existingPriests = await _uow.ParishMember.GetAllAsync(pm => existingPriestIds.Contains(pm.Id));
            
            foreach (PlanPriestDto newPriest in planDto.ActivePriests)
            {
                ParishMember? priest = existingPriests.FirstOrDefault(pm => pm.Id == newPriest.Id);
                if (priest is null)
                {
                    // Utwórz nowego użytkownika z generycznymi danymi
                    User centralUser = await _uow.ParishMember.GenerateUserAsync(newPriest.DisplayName);
                    priest = new()
                    {
                        DisplayName = newPriest.DisplayName,
                        CentralUserId = centralUser.Id,
                    };
                    var result = await _userManager.CreateAsync(centralUser);

                    if (result.Succeeded)
                    {
                        _uow.ParishMember.Add(priest);
                        await _userManager.AddToRoleAsync(centralUser, Role.Priest.ToString());
                    }
                    else
                    {
                        continue;
                    }
                }

                // Dodaj użytkownika do planu
                plan.ActivePriests.Add(priest);
            }

            await _uow.SaveAsync();
            await transaction.CommitAsync();
        }
    }
}
