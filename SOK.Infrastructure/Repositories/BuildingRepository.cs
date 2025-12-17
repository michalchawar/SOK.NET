using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using SOK.Application.Common.Interface;
using SOK.Domain.Entities.Parish;
using SOK.Infrastructure.Persistence.Context;

namespace SOK.Infrastructure.Repositories
{
    /// <inheritdoc />
    public class BuildingRepository : UpdatableRepository<Building, ParishDbContext>, IBuildingRepository
    {
        public BuildingRepository(ParishDbContext db) : base(db) {}
        
        /// <inheritdoc />
        public async Task<IEnumerable<Building>> GetPaginatedAsync(
            Expression<Func<Building, bool>>? filter,
            int pageSize = 1,
            int page = 1,
            bool street = false,
            bool addresses = false,
            bool days = false,
            bool tracked = false)
        {
            var query = GetQueryable(filter: filter, tracked: tracked);

            if (pageSize < 1) throw new ArgumentException("Page size must be positive.");
            if (page < 1) throw new ArgumentException("Page must be positive.");

            query = query.Skip((page - 1) * pageSize);
            query = query.Take(pageSize);

            if (street)
                query = query
                    .Include(b => b.Street)
                        .ThenInclude(s => s.City)
                    .Include(b => b.Street)
                        .ThenInclude(s => s.Type);

            if (addresses)
                query = query.Include(b => b.Addresses)
                         .ThenInclude(a => a.Submissions);

            if (days)
                query = query.Include(b => b.Days);

            return await query.ToListAsync();
        }
    }
}
