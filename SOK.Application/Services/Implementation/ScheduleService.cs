using SOK.Application.Common.Interface;
using SOK.Application.Services.Interface;
using SOK.Domain.Entities.Parish;
using System.Linq.Expressions;
using System.Numerics;

namespace SOK.Application.Services.Implementation
{
    /// <inheritdoc />
    public class ScheduleService : IScheduleService
    {
        private readonly IUnitOfWorkParish _uow;

        public ScheduleService(IUnitOfWorkParish uow)
        {
            _uow = uow;
        }

        /// <inheritdoc />
        public async Task<Schedule?> GetScheduleAsync(int id)
        {
            return await _uow.Schedule.GetAsync(s => s.Id == id);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<Schedule>> GetActiveSchedules()
        {
            string? activePlanIdStr = await _uow.ParishInfo.GetValueAsync("ActivePlanId");

            if (string.IsNullOrEmpty(activePlanIdStr))
                throw new ArgumentException("No active plan is set.");

            int activePlanId = int.Parse(activePlanIdStr);
            Plan? activePlan = await _uow.Plan.GetAsync(p => p.Id == activePlanId, includeProperties: "Schedules");

            return activePlan is not null ? new List<Schedule>(activePlan.Schedules) : new List<Schedule>();
        }

        /// <inheritdoc />
        public async Task CreateScheduleAsync(Schedule schedule)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public async Task<bool> DeleteScheduleAsync(int id)
        {
            try
            {
                Schedule? schedule = await _uow.Schedule.GetAsync(s => s.Id == id);
                if (schedule != null)
                {
                    _uow.Schedule.Remove(schedule);
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
        public async Task UpdateScheduleAsync(Schedule schedule)
        {
            _uow.Schedule.Update(schedule);
            await _uow.SaveAsync();
        }

        /// <inheritdoc />
        public async Task SetDefaultScheduleAsync(Schedule schedule)
        {
            Schedule? entity = await _uow.Schedule.GetAsync(s => s.Id == schedule.Id, includeProperties: "Plan");
            string? activePlanId = await _uow.ParishInfo.GetValueAsync("ActivePlanId");

            if (entity is null)
                throw new ArgumentException("Plan does not exist in the database.");
            if (string.IsNullOrEmpty(activePlanId))
                throw new ArgumentException("No active plan is set.");
            if (entity.PlanId.ToString() != activePlanId)
                throw new ArgumentException("Schedule doesn't belong to active plan.");

            await _uow.ParishInfo.SetValueAsync("DefaultScheduleId", schedule.Id.ToString());
            await _uow.SaveAsync();
        }

        /// <inheritdoc />
        public async Task<Schedule?> GetDefaultScheduleAsync()
        {
            string? scheduleIdStr = await _uow.ParishInfo.GetValueAsync("DefaultScheduleId");

            if (string.IsNullOrEmpty(scheduleIdStr))
            {
                return null;
            }

            int scheduleId = int.Parse(scheduleIdStr);
            return await _uow.Schedule.GetAsync(s => s.Id == scheduleId);
        }

        /// <inheritdoc />
        public async Task ClearDefaultScheduleAsync()
        {
            await _uow.ParishInfo.ClearValueAsync("DefaultScheduleId");
            await _uow.SaveAsync();
        }
    }
}
