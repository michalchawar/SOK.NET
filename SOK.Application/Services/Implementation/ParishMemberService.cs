using Microsoft.AspNetCore.Identity;
using SOK.Application.Common.DTO;
using SOK.Application.Common.Interface;
using SOK.Application.Services.Interface;
using SOK.Domain.Entities.Central;
using SOK.Domain.Entities.Parish;
using SOK.Domain.Enums;
using System.Drawing;
using System.Linq.Expressions;
using System.Security.Claims;

namespace SOK.Application.Services.Implementation
{
    /// <inheritdoc />
    public class ParishMemberService : IParishMemberService
    {
        private readonly IUnitOfWorkParish _uow;
        private readonly UserManager<User> _userManager;

        public ParishMemberService(IUnitOfWorkParish uow, UserManager<User> userManager)
        {
            _uow = uow;
            _userManager = userManager;
        }

        /// <inheritdoc />
        public async Task<ParishMember?> GetParishMemberAsync(int id)
        {
            return await _uow.ParishMember.GetAsync(pm => pm.Id == id);
        }

        /// <inheritdoc />
        public async Task<ParishMember?> GetParishMemberAsync(ClaimsPrincipal userClaim)
        {
            bool parishMemberFound = int.TryParse(_userManager.GetUserId(userClaim), out int userId);
            return parishMemberFound ? await _uow.ParishMember.GetAsync(pm => pm.Id == userId) : null;
        }

        /// <inheritdoc />
        public async Task UpdateParishMemberAsync(ParishMember parishMember)
        {
            _uow.ParishMember.Update(parishMember);
            await _uow.SaveAsync();
        }

        /// <inheritdoc />
        public async Task<IEnumerable<ParishMember>> GetAllInRoleAsync(Role role)
        {
            var centralPriestIds = (await _userManager.GetUsersInRoleAsync(role.ToString())).Select(u => u.Id);
            var parishMembers = await _uow.ParishMember.GetAllAsync(
                pm => centralPriestIds.Contains(pm.CentralUserId.ToString()),
                includeProperties: "AssignedPlans");

            return parishMembers;
        }

        /// <inheritdoc />
        public async Task<IEnumerable<ParishMember>> GetAllParishMembersAsync(
            Expression<Func<ParishMember, bool>>? filter = null,
            bool agendas = false,
            bool plans = false,
            bool submissions = false)
        {
            string includeProperties = "";

            if (agendas) includeProperties += "AssignedAgendas, ";
            if (plans) includeProperties += "AssignedPlans, ";
            if (submissions) includeProperties += "EnteredSubmissions";

            return await _uow.ParishMember.GetAllAsync(
                filter: filter,
                includeProperties: includeProperties
            );
        }

        public async Task<List<UserDto>> GetUsersPaginatedAsync(
            Expression<Func<User, bool>>? filter = null, 
            int page = 1, 
            int pageSize = 1,
            bool loadRoles = false)
        {
            if (pageSize < 1) throw new ArgumentException("Page size must be positive.");
            if (page < 1) throw new ArgumentException("Page must be positive.");

            List<UserDto> result = [.. await _uow.ParishMember.GetPaginatedAsync(
                    filter,
                    pageSize: pageSize,
                    page: page,
                    roles: loadRoles)];

            return result;
        }
    }
}