using SOK.Application.Common.Interface;
using SOK.Application.Services.Interface;
using SOK.Domain.Entities.Parish;
using System.Linq.Expressions;

namespace SOK.Application.Services.Implementation
{
    /// <inheritdoc />
    public class PlanService : IPlanService
    {
        private readonly IUnitOfWorkParish _uow;

        public PlanService(IUnitOfWorkParish uow)
        {
            _uow = uow;
        }

        /// <inheritdoc />
        public async Task<Plan?> GetPlanAsync(int id)
        {
            return await _uow.Plan.GetAsync(p => p.Id == id);
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
            throw new NotImplementedException();
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
            catch (Exception ex)
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

            if (entity is null) { 
                throw new ArgumentException("Plan does not exist in the database.");
            }

            await _uow.ParishInfo.SetValueAsync("ActivePlanId", plan.Id.ToString());

            Schedule? defaultSchedule = entity.Schedules.FirstOrDefault(s => s.ShortName == "T") ?? entity.Schedules.FirstOrDefault();
            
            if (defaultSchedule is not null)
                await _uow.ParishInfo.SetValueAsync("DefaultScheduleId", 
                    (entity.Schedules.FirstOrDefault(s => s.ShortName == "T") ?? entity.Schedules.First()).Id.ToString());
            else
                await _uow.ParishInfo.ClearValueAsync("DefaultScheduleId");

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
    }
}
