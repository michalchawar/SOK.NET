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
            bool plan = false,
            bool tracked = false)
        {
            var query = GetQueryable(filter: filter, tracked: tracked);

            if (pageSize < 1) throw new ArgumentException("Page size must be positive.");
            if (page < 1) throw new ArgumentException("Page must be positive.");

            query = query.OrderByDescending(s => s.SubmitTime);

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
            
            if (plan)
                query = query.Include(s => s.Plan);

            return await query.ToListAsync();
        }

        public async Task<(IEnumerable<Submission> submissions, int totalCount)> GetPaginatedWithSortingAsync(
            Expression<Func<Submission, bool>>? filter,
            string sortBy = "time",
            string order = "desc",
            int pageSize = 1,
            int page = 1,
            bool submitter = false,
            bool address = false,
            bool addressFull = false,
            bool visit = false,
            bool history = false,
            bool formSubmission = false,
            bool plan = false,
            bool tracked = false)
        {
            var query = GetQueryable(filter: filter, tracked: tracked);

            if (pageSize < 1) throw new ArgumentException("Page size must be positive.");
            if (page < 1) throw new ArgumentException("Page must be positive.");

            // Include relations needed for sorting and display
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

            if (plan)
                query = query.Include(s => s.Plan);

            // Apply sorting
            bool ascending = order.ToLower() == "asc";

            query = sortBy.ToLower() switch
            {
                "address" => ascending
                    ? query.OrderBy(s => s.Address.StreetName)
                           .ThenBy(s => s.Address.BuildingNumber)
                           .ThenBy(s => s.Address.BuildingLetter ?? "")
                           .ThenBy(s => s.Address.ApartmentNumber ?? 0)
                           .ThenBy(s => s.Address.ApartmentLetter ?? "")
                    : query.OrderByDescending(s => s.Address.StreetName)
                           .ThenByDescending(s => s.Address.BuildingNumber)
                           .ThenByDescending(s => s.Address.BuildingLetter ?? "")
                           .ThenByDescending(s => s.Address.ApartmentNumber ?? 0)
                           .ThenByDescending(s => s.Address.ApartmentLetter ?? ""),

                "submitter" => ascending
                    ? query.OrderBy(s => s.Submitter.Surname)
                           .ThenBy(s => s.Submitter.Name)
                    : query.OrderByDescending(s => s.Submitter.Surname)
                           .ThenByDescending(s => s.Submitter.Name),

                "time" or _ => ascending
                    ? query.OrderBy(s => s.SubmitTime)
                    : query.OrderByDescending(s => s.SubmitTime)
            };

            // Count total before pagination
            int totalCount = await query.CountAsync();

            // Apply pagination
            query = query.Skip((page - 1) * pageSize).Take(pageSize);

            var submissions = await query.ToListAsync();

            return (submissions, totalCount);
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
