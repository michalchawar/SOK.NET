using Microsoft.EntityFrameworkCore;
using SOK.Application.Common.Interface;
using SOK.Application.Services.Interface;
using SOK.Domain.Entities.Central;
using SOK.Domain.Entities.Parish;
using SOK.Infrastructure.Persistence.Context;
using System.Linq.Expressions;

namespace SOK.Infrastructure.Repositories
{
    public class ParishMemberRepository : Repository<ParishMember, ParishDbContext>, IParishMemberRepository
    {
        private readonly ParishDbContext _db;

        public ParishMemberRepository(ParishDbContext db) : base(db)
        {
            _db = db;
        }

        public async Task<IEnumerable<ParishMember>> GetPaginatedAsync(
            Expression<Func<ParishMember, bool>>? filter, 
            int pageSize = 1, 
            int page = 1, 
            bool assignedAgendas = false, 
            bool enteredSubmissions = false, 
            bool tracked = false)
        {
            var query = GetQueryable(filter: filter, tracked: tracked);

            if (pageSize < 1) throw new ArgumentException("Page size must be positive.");
            if (page < 1) throw new ArgumentException("Page must be positive.");
            query = query.Skip((page - 1) * pageSize);
            query = query.Take(pageSize);

            if (assignedAgendas)
                query = query.Include(pm => pm.AssignedAgendas);

            if (enteredSubmissions)
                query = query.Include(pm => pm.EnteredSubmissions);

            return await query.ToListAsync();
        }

        public void Update(ParishMember parishMember)
        {
            dbSet.Update(parishMember);
        }

        public async Task<User> GenerateUserAsync(string displayName)
        {
            Random random = new();

            ParishEntry? parish = await _db.GetCurrentParishAsync();
            if (parish is null)
                throw new InvalidOperationException("Cannot create generic user: There is no active parish set.");

            return new User()
            {
                UserName = string.Join("-", displayName.ToLower().Replace(".", string.Empty).Replace(",", string.Empty).Split([' ', '-', '/'], StringSplitOptions.TrimEntries).TakeLast(2).Append(random.Next(1000).ToString())),
                Parish = parish,
            };
        }
    }
}
