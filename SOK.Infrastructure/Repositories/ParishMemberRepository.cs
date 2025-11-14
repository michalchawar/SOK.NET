using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Diagnostics;
using Microsoft.EntityFrameworkCore;
using SOK.Application.Common.Interface;
using SOK.Application.Services.Interface;
using SOK.Domain.Entities.Central;
using SOK.Domain.Entities.Parish;
using SOK.Domain.Enums;
using SOK.Infrastructure.Persistence.Context;
using System.Linq.Expressions;

namespace SOK.Infrastructure.Repositories
{
    /// <inheritdoc />
    public class ParishMemberRepository : UpdatableRepository<ParishMember, ParishDbContext>, IParishMemberRepository
    {
        private readonly UserManager<User> _userManager;

        public ParishMemberRepository(ParishDbContext db, UserManager<User> userManager) : base(db)
        {
            _userManager = userManager;
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

        /// <inheritdoc />
        public async Task<ParishMember?> CreateMemberWithUserAccountAsync(string displayName, IEnumerable<Role> roles)
        {
            Random random = new();

            ParishEntry? parish = await _db.GetCurrentParishAsync();
            if (parish is null)
                throw new InvalidOperationException("Cannot create new parish member: There is no active parish set.");

            User newUser = new()
            {
                UserName = string.Join("-", displayName.ToLower().Replace(".", string.Empty).Replace(",", string.Empty).Split([' ', '-', '/'], StringSplitOptions.TrimEntries).TakeLast(2).Append(random.Next(1000).ToString())),
                DisplayName = displayName,
                Parish = parish,
            };

            ParishMember newMember = new()
            {
                DisplayName = newUser.DisplayName,
                CentralUserId = newUser.Id,
            };

            var result = await _userManager.CreateAsync(newUser);

            if (!result.Succeeded)
                return null;

            dbSet.Add(newMember);
            await _userManager.AddToRolesAsync(newUser, roles.Select(r => r.ToString()));

            return newMember;
        }
    }
}
