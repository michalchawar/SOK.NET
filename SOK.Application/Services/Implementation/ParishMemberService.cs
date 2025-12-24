using Microsoft.AspNetCore.Identity;
using SOK.Application.Common.DTO;
using SOK.Application.Common.Helpers;
using SOK.Application.Common.Interface;
using SOK.Application.Services.Interface;
using SOK.Domain.Entities.Central;
using SOK.Domain.Entities.Parish;
using SOK.Domain.Enums;
using System.Drawing;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace SOK.Application.Services.Implementation
{
    /// <inheritdoc />
    public class ParishMemberService : IParishMemberService
    {
        private readonly IUnitOfWorkParish _uow;
        private readonly IUnitOfWorkCentral _uowCentral;
        private readonly UserManager<User> _userManager;

        public ParishMemberService(IUnitOfWorkParish uow, IUnitOfWorkCentral uowCentral, UserManager<User> userManager)
        {
            _uow = uow;
            _uowCentral = uowCentral;
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
            string? userUid = _userManager.GetUserId(userClaim);
            return userUid != null ? await _uow.ParishMember.GetAsync(pm => pm.CentralUserId.ToString() == userUid) : null;
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

        /// <inheritdoc />
        public async Task<UserDto?> CreateUserAsync(string displayName, string userName, string? email, string password, IEnumerable<Role> roles)
        {
            // Check if username already exists
            var existingUser = await _userManager.FindByNameAsync(userName);
            if (existingUser != null)
                return null;

            // Wykonaj operacje w ramach transakcji z obsługą retry strategy
            return await _uowCentral.ExecuteInTransactionAsync(async () =>
            {
                return await _uow.ExecuteInTransactionAsync(async () =>
                {
                    var member = await _uow.ParishMember.CreateMemberWithUserAccountAsync(displayName, roles);
                    if (member == null)
                        return null;

                    var user = await _uow.ParishMember.GetUserByIdAsync(member.CentralUserId);
                    if (user == null)
                        return null;

                    // Update username and email
                    user.UserName = userName;
                    user.Email = email;
                    await _uow.ParishMember.UpdateUserAsync(user);

                    // Set password
                    var passwordResult = await _uow.ParishMember.SetPasswordAsync(user, password);
                    if (!passwordResult.Succeeded)
                        return null;

                    await _uow.SaveAsync();

                    // Return UserDto
                    var userRoles = await _userManager.GetRolesAsync(user);
                    return new UserDto
                    {
                        CentralId = user.Id,
                        ParishId = member.Id,
                        DisplayName = displayName,
                        UserName = userName,
                        Email = email,
                        Roles = userRoles.Select(r => Enum.Parse<Role>(r)).ToList()
                    };
                });
            });
        }

        /// <inheritdoc />
        public async Task<bool> UpdateUserAsync(string userId, string displayName, string userName, string? email, IEnumerable<Role> roles, IEnumerable<int> assignedPlanIds)
        {
            var user = await _uow.ParishMember.GetUserByIdAsync(userId);
            if (user == null)
                return false;

            var member = await _uow.ParishMember.GetAsync(pm => pm.CentralUserId == userId, tracked: true);
            if (member == null)
                return false;

            // Wykonaj operacje w ramach transakcji z obsługą retry strategy
            return await _uowCentral.ExecuteInTransactionAsync(async () =>
            {
                return await _uow.ExecuteInTransactionAsync(async () =>
                {
                    // Update User
                    user.UserName = userName;
                    user.Email = email;
                    user.DisplayName = displayName;
                    
                    await _uow.ParishMember.UpdateUserAsync(user);

                    // Update ParishMember
                    member.DisplayName = displayName;

                    // Update roles
                    var currentRoles = await _userManager.GetRolesAsync(user);
                    var rolesToRemove = currentRoles.Except(roles.Select(r => r.ToString()));
                    var rolesToAdd = roles.Select(r => r.ToString()).Except(currentRoles);

                    if (rolesToRemove.Any())
                        await _userManager.RemoveFromRolesAsync(user, rolesToRemove);
                    
                    if (rolesToAdd.Any())
                        await _userManager.AddToRolesAsync(user, rolesToAdd);

                    // Update assigned plans
                    var allPlans = await _uow.Plan.GetAllAsync(includeProperties: "ActivePriests", tracked: true);
                    
                    // Remove from plans not in the list
                    foreach (var plan in allPlans)
                    {
                        if (plan.ActivePriests.Any(p => p.Id == member.Id) && !assignedPlanIds.Contains(plan.Id))
                        {
                            plan.ActivePriests.Remove(member);
                        }
                    }

                    // Add to plans in the list
                    foreach (var planId in assignedPlanIds)
                    {
                        var plan = allPlans.FirstOrDefault(p => p.Id == planId);
                        if (plan != null && !plan.ActivePriests.Any(p => p.Id == member.Id))
                        {
                            plan.ActivePriests.Add(member);
                        }
                    }
                    
                    await _uow.SaveAsync();
                    return true;
                });
            });
        }

        /// <inheritdoc />
        public async Task<string?> ResetPasswordAsync(string userId)
        {
            var user = await _uow.ParishMember.GetUserByIdAsync(userId);
            if (user == null)
                return null;

            // Generate random password
            string newPassword = GenerateRandomPassword(12);

            var result = await _uow.ParishMember.SetPasswordAsync(user, newPassword);
            if (!result.Succeeded)
                return null;

            await _uow.SaveAsync();
            return newPassword;
        }

        /// <inheritdoc />
        public async Task<UserDto?> GetUserByIdAsync(string userId, bool loadRoles = false, bool loadPlans = false)
        {
            var user = await _uow.ParishMember.GetUserByIdAsync(userId);
            if (user == null)
                return null;

            var member = await _uow.ParishMember.GetAsync(
                pm => pm.CentralUserId == userId,
                includeProperties: loadPlans ? "AssignedPlans" : "");

            if (member == null)
                return null;

            List<Role> userRoles = new();
            if (loadRoles)
            {
                var roleNames = await _userManager.GetRolesAsync(user);
                userRoles = roleNames.Select(r => Enum.Parse<Role>(r)).ToList();
            }

            return new UserDto
            {
                CentralId = user.Id,
                ParishId = member.Id,
                DisplayName = member.DisplayName,
                UserName = user.UserName,
                Email = user.Email,
                Roles = userRoles,
                AssignedPlans = loadPlans ? member.AssignedPlans : new List<Plan>()
            };
        }

        /// <inheritdoc />
        public async Task<IEnumerable<Plan>> GetAllPlansAsync()
        {
            return await _uow.Plan.GetAllAsync();
        }

        private static string GenerateRandomPassword(int length)
        {
            const string validChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890!@#$%^&*";
            StringBuilder password = new StringBuilder();
            
            using (var rng = RandomNumberGenerator.Create())
            {
                byte[] randomBytes = new byte[length];
                rng.GetBytes(randomBytes);
                
                foreach (byte b in randomBytes)
                {
                    password.Append(validChars[b % validChars.Length]);
                }
            }

            return password.ToString();
        }

        // === Metody metadanych ===

        /// <inheritdoc />
        public async Task<int?> GetIntMetadataAsync(ParishMember member, string metadataKey)
        {
            string? value = await _uow.ParishInfo.GetMetadataAsync(member, metadataKey);
            if (string.IsNullOrEmpty(value))
                return null;

            if (int.TryParse(value, out int result))
                return result;

            return null;
        }

        /// <inheritdoc />
        public async Task SetIntMetadataAsync(ParishMember member, string metadataKey, int value)
        {
            await _uow.ParishInfo.SetMetadataAsync(member, metadataKey, value.ToString());
            await _uow.SaveAsync();
        }

        /// <inheritdoc />
        public async Task DeleteMetadataAsync(ParishMember member, string metadataKey)
        {
            await _uow.ParishInfo.DeleteMetadataAsync(member, metadataKey);
            await _uow.SaveAsync();
        }
    }
}