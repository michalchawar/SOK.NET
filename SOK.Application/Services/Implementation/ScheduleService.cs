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
            // Pobierz aktywny plan
            Plan? activePlan = await GetActivePlan(includeProperties: "Schedules");

            if (activePlan is null)
                throw new ArgumentException("No active plan is set.");

            return [.. activePlan.Schedules];
        }

        /// <inheritdoc />
        public Task CreateScheduleAsync(Schedule schedule)
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
            catch (Exception)
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
            // Pobierz plan powiązany z harmonogramem
            Schedule? entity = await _uow.Schedule.GetAsync(s => s.Id == schedule.Id, includeProperties: "Plan");

            // Pobierz aktywny plan
            Plan? activePlan = await GetActivePlan(includeProperties: "DefaultSchedule");
            
            if (entity is null)
                throw new ArgumentException("Plan does not exist in the database.");
            if (activePlan is null)
                throw new ArgumentException("No active plan is set.");
            if (entity.PlanId != activePlan.Id)
                throw new ArgumentException("Schedule doesn't belong to active plan.");

            activePlan.DefaultSchedule = entity;
            _uow.Plan.Update(activePlan);

            await _uow.SaveAsync();
        }

        /// <inheritdoc />
        public async Task<Schedule?> GetDefaultScheduleAsync()
        {
            // Pobierz aktywny plan
            Plan? activePlan = await GetActivePlan(includeProperties: "DefaultSchedule");

            if (activePlan is null)
                throw new ArgumentException("No active plan is set.");

            return activePlan.DefaultSchedule;
        }

        /// <inheritdoc />
        public async Task ClearDefaultScheduleAsync()
        {
            await _uow.ParishInfo.ClearValueAsync("DefaultScheduleId");
            await _uow.SaveAsync();
        }
        
        /// <summary>
        /// Pobiera aktywny plan systemu.
        /// </summary>
        /// <param name="includeProperties">Łańcuch właściwości do dołączenia.</param>
        /// <returns>
        /// Obiekt <see cref="Task"/>, reprezentujący asynchroniczną operację,
        /// którego wartością jest aktywny plan lub <see cref="null"/>, jeśli nie ma aktywnego planu. 
        /// </returns>
        private async Task<Plan?> GetActivePlan(string includeProperties = "")
        {
            string? activePlanId = await _uow.ParishInfo.GetValueAsync("ActivePlanId");
            return await _uow.Plan.GetAsync(p => p.Id.ToString() == activePlanId, includeProperties: includeProperties);
        }
    }
}
