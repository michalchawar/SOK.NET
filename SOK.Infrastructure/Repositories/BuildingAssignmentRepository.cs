using Microsoft.EntityFrameworkCore;
using SOK.Application.Common.Interface;
using SOK.Domain.Entities.Parish;
using SOK.Infrastructure.Persistence;
using SOK.Infrastructure.Persistence.Context;

namespace SOK.Infrastructure.Repositories
{
    /// <inheritdoc />
    public class BuildingAssignmentRepository : UpdatableRepository<BuildingAssignment, ParishDbContext>, IBuildingAssignmentRepository
    {
        public BuildingAssignmentRepository(ParishDbContext db) : base(db)
        {
        }

        /// <inheritdoc />
        public async Task<List<BuildingAssignment>> GetAssignmentsForDayAsync(int dayId)
        {
            return await GetQueryable()
                .Where(ba => ba.DayId == dayId)
                .Include(ba => ba.Building)
                    .ThenInclude(b => b.Street)
                .Include(ba => ba.Schedule)
                .ToListAsync();
        }

        /// <inheritdoc />
        public async Task<BuildingAssignment?> GetAssignmentAsync(int dayId, int buildingId, int scheduleId)
        {
            return await GetQueryable()
                .FirstOrDefaultAsync(ba => 
                    ba.DayId == dayId && 
                    ba.BuildingId == buildingId && 
                    ba.ScheduleId == scheduleId);
        }

        /// <inheritdoc />
        public async Task<bool> IsBuildingAssignedAsync(int buildingId, int scheduleId)
        {
            return await GetQueryable()
                .AnyAsync(ba => ba.BuildingId == buildingId && ba.ScheduleId == scheduleId);
        }
    }
}
