using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Diagnostics;
using Microsoft.EntityFrameworkCore;
using SOK.Application.Common.DTO;
using SOK.Application.Common.Helpers;
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
        private Random _random;

        public ParishMemberRepository(ParishDbContext db, UserManager<User> userManager) : base(db)
        {
            _userManager = userManager;
            _random = new Random();
        }

        // public async Task<IEnumerable<ParishMember>> GetPaginatedAsync(
        //     Expression<Func<ParishMember, bool>>? filter, 
        //     int pageSize = 1, 
        //     int page = 1, 
        //     bool assignedAgendas = false, 
        //     bool enteredSubmissions = false, 
        //     bool tracked = false)
        // {
        //     var query = GetQueryable(filter: filter, tracked: tracked);

        //     if (pageSize < 1) throw new ArgumentException("Page size must be positive.");
        //     if (page < 1) throw new ArgumentException("Page must be positive.");
        //     query = query.Skip((page - 1) * pageSize);
        //     query = query.Take(pageSize);

        //     if (assignedAgendas)
        //         query = query.Include(pm => pm.AssignedAgendas);

        //     if (enteredSubmissions)
        //         query = query.Include(pm => pm.EnteredSubmissions);

        //     return await query.ToListAsync();
        // }
        
        public async Task<IEnumerable<UserDto>> GetPaginatedAsync(
            Expression<Func<User, bool>>? filter, 
            int pageSize = 1, 
            int page = 1,
            bool roles = false,
            bool assignedAgendas = false,
            bool assignedPlans = false,
            bool enteredSubmissions = false)
        {
            var query = filter is not null ? 
                    _userManager.Users.Where(filter) :
                    _userManager.Users;

            if (pageSize < 1) throw new ArgumentException("Page size must be positive.");
            if (page < 1) throw new ArgumentException("Page must be positive.");
            query = query.Skip((page - 1) * pageSize);
            query = query.Take(pageSize);

            var users = await query.ToListAsync();
            var userIds = users.Select(u => u.Id).ToList();

            var membersQuery = dbSet.Where(pm => userIds.Contains(pm.CentralUserId));

            if (assignedAgendas)
                membersQuery = membersQuery.Include(pm => pm.AssignedAgendas);

            if (assignedPlans)
                membersQuery = membersQuery.Include(pm => pm.AssignedPlans);

            if (enteredSubmissions)
                membersQuery = membersQuery.Include(pm => pm.EnteredSubmissions);

            var members = await membersQuery.ToListAsync();

            List<UserDto> result = new();
            foreach (User user in users)
            {
                ParishMember? member = members.FirstOrDefault(pm => pm.CentralUserId == user.Id);
                
                if (member is null)
                    continue;

                List<Role> userRoles = new(); 
                if (roles)
                    userRoles = [..(await _userManager.GetRolesAsync(user)).Select(roleName => Enum.Parse<Role>(roleName))];
                
                result.Add(new UserDto()
                {
                    CentralId = user.Id,
                    ParishId = member.Id,
                    UserName = user.UserName,
                    DisplayName = member.DisplayName,
                    Email = user.Email,
                    Roles = userRoles,
                    AssignedAgendas = member.AssignedAgendas,
                    AssignedPlans = member.AssignedPlans,
                    EnteredSubmissions = member.EnteredSubmissions
                });
            }

            return result;
        }

        /// <inheritdoc />
        public async Task<ParishMember?> CreateMemberWithUserAccountAsync(string displayName, IEnumerable<Role> roles)
        {
            ParishEntry? parish = await _db.GetCurrentParishAsync();
            if (parish is null)
                throw new InvalidOperationException("Cannot create new parish member: There is no active parish set.");

            User newUser = new()
            {
                UserName = CreateUserNameFromDisplayName(displayName),
                DisplayName = displayName,
                ParishId = parish.Id,
            };

            var result = await _userManager.CreateAsync(newUser);

            if (!result.Succeeded)
                return null;

            ParishMember newMember = new()
            {
                DisplayName = newUser.DisplayName,
                CentralUserId = newUser.Id,
            };

            dbSet.Add(newMember);
            await _userManager.AddToRolesAsync(newUser, roles.Select(r => r.ToString()));

            return newMember;
        }

        /// <inheritdoc />
        public async Task<User?> GetUserByIdAsync(string userId)
        {
            return await _userManager.FindByIdAsync(userId);
        }

        /// <inheritdoc />
        public async Task UpdateUserAsync(User user)
        {
            await _userManager.UpdateAsync(user);
        }

        /// <inheritdoc />
        public async Task<IdentityResult> SetPasswordAsync(User user, string newPassword)
        {
            // Remove old password if exists
            if (await _userManager.HasPasswordAsync(user))
            {
                await _userManager.RemovePasswordAsync(user);
            }
            
            return await _userManager.AddPasswordAsync(user, newPassword);
        }

        private string CreateUserNameFromDisplayName(string displayName)
        {
            char joinCharacter = '-';

            string baseUserName = displayName.Replace(" ", string.Empty).Replace(",", string.Empty).Replace(".", string.Empty).NormalizePolishDiacritics().ToLower();
            string userName = string.Join(joinCharacter, baseUserName.Split([' ', '-', '/'], StringSplitOptions.TrimEntries).TakeLast(2));
            int suffix = 1;

            while (_userManager.FindByNameAsync(userName).Result is not null)
            {
                userName = $"{baseUserName}{joinCharacter}{_random.Next(1000, 9999)}";
                suffix++;
            }

            return userName;
        }
    }
}
