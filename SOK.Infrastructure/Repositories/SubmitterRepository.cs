using Microsoft.EntityFrameworkCore;
using SOK.Application.Common.Interface;
using SOK.Domain.Entities.Parish;
using SOK.Infrastructure.Persistence.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SOK.Infrastructure.Repositories
{
    public class SubmitterRepository : Repository<Submitter, ParishDbContext>, ISubmitterRepository
    {
        private readonly ParishDbContext _db;

        public SubmitterRepository(ParishDbContext db) : base(db)
        {
            _db = db;
        }

        public async Task<IEnumerable<Submitter>> GetPaginatedAsync(
            Expression<Func<Submitter, bool>>? filter, 
            int pageSize = 1, 
            int page = 1, 
            bool submissions = false, 
            bool tracked = false)
        {
            var query = GetQueryable(filter: filter, tracked: tracked);

            if (pageSize < 1) throw new ArgumentException("Page size must be positive.");
            if (page < 1) throw new ArgumentException("Page must be positive.");
            query = query.Skip((page - 1) * pageSize);
            query = query.Take(pageSize);

            if (submissions)
                query = query.Include(s => s.Submissions);

            return await query.ToListAsync();
        }

        public void Update(Submitter submitter)
        {
            dbSet.Update(submitter);
        }
    }
}
