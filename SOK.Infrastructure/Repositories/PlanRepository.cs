using Microsoft.EntityFrameworkCore;
using SOK.Application.Common.Interface;
using SOK.Domain.Entities.Parish;
using SOK.Infrastructure.Persistence.Context;
using System.Linq.Expressions;

namespace SOK.Infrastructure.Repositories
{
    public class PlanRepository : Repository<Plan, ParishDbContext>, IPlanRepository
    {
        private readonly ParishDbContext _db;
        public PlanRepository(ParishDbContext db) : base(db)
        {
            _db = db;
        }

        public async Task<IEnumerable<Plan>> GetPaginatedAsync(
            Expression<Func<Plan, bool>>? filter, 
            int pageSize = 1, 
            int page = 1, 
            bool author = false, 
            bool schedules = false, 
            bool submissions = false, 
            bool days = false,
            bool tracked = false)
        {
            var query = GetQueryable(filter: filter, tracked: tracked);

            if (pageSize < 1) throw new ArgumentException("Page size must be positive.");
            if (page < 1) throw new ArgumentException("Page must be positive.");
            query = query.Skip((page - 1) * pageSize);
            query = query.Take(pageSize);

            if (author)
                query = query.Include(p => p.Author);

            if (schedules)
                query = query.Include(p => p.Schedules);

            if (submissions)
                query = query.Include(p => p.Submissions);

            if (days)
                query = query.Include(p => p.Days);

            return await query.ToListAsync();
        }

        public void Update(Plan plan)
        {
            dbSet.Update(plan);
        }
    }
}
