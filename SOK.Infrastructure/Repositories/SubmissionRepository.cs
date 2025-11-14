using Microsoft.EntityFrameworkCore;
using SOK.Application.Common.Interface;
using SOK.Domain.Entities.Parish;
using SOK.Infrastructure.Persistence.Context;
using System.Linq.Expressions;

namespace SOK.Infrastructure.Repositories
{
    /// <inheritdoc />
    public class SubmissionRepository : UpdatableRepository<Submission, ParishDbContext>, ISubmissionRepository
    {
        public SubmissionRepository(ParishDbContext db) : base(db) {}

        public async Task<IEnumerable<Submission>> GetPaginatedAsync(
            Expression<Func<Submission, bool>>? filter,
            int pageSize = 1,
            int page = 1,
            bool submitter = false, 
            bool address = false,
            bool addressFull = false,
            bool visit = false,
            bool history = false, 
            bool formSubmission = false,
            bool tracked = false)
        {
            var query = GetQueryable(filter: filter, tracked: tracked);

            if (pageSize < 1) throw new ArgumentException("Page size must be positive.");
            if (page < 1) throw new ArgumentException("Page must be positive.");
            query = query.Skip((page - 1) * pageSize);
            query = query.Take(pageSize);

            if (submitter)
                query = query.Include(s => s.Submitter);

            if (address)
                query = query.Include(s => s.Address);

            if (addressFull) 
                query = query
                    .Include(s => s.Address)
                        .ThenInclude(a => a.Building)
                            .ThenInclude(b => b.Street)
                                .ThenInclude(s => s.City)
                    .Include(s => s.Address)
                        .ThenInclude(a => a.Building)
                            .ThenInclude(b => b.Street)
                                .ThenInclude(s => s.Type);

            if (visit)
                query = query
                    .Include(s => s.Visit)
                        .ThenInclude(v => v.Schedule)
                    .Include(s => s.Visit)
                        .ThenInclude(v => v.Agenda)
                            .ThenInclude(a => a!.Day);
            
            if (history)
                query = query.Include(s => s.History);

            if (formSubmission)
                query = query.Include(s => s.FormSubmission);

            return await query.ToListAsync();
        }

        public async Task<Submission?> GetRandomAsync()
        {
            var query = GetQueryable(tracked: false);
            query = query
                .Include(s => s.Address)
                .Include(s => s.Submitter);

            var rand = new Random();
            var skipCount = (int)(rand.NextDouble() * dbSet.Count());

            query = query.Skip(skipCount);

            return await query.FirstOrDefaultAsync();
        }
    }
}
