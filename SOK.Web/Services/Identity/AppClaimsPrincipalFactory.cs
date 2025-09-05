using SOK.Web.Data;
using SOK.Web.Models.Central.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace SOK.Web.Services.Identity
{
    public class AppClaimsPrincipalFactory : UserClaimsPrincipalFactory<User, IdentityRole>
    {
        private readonly CentralDbContext _dbContext;

        public AppClaimsPrincipalFactory(
            UserManager<User> userManager,
            RoleManager<IdentityRole> roleManager,
            IOptions<IdentityOptions> optionsAccessor,
            CentralDbContext _context)
            : base(userManager, roleManager, optionsAccessor) 
        {
            _dbContext = _context;
        }

        protected override async Task<ClaimsIdentity> GenerateClaimsAsync(User user)
        {
            var identity = await base.GenerateClaimsAsync(user);

            // Dodanie dodatkowych claimów do tożsamości użytkownika
            identity.AddClaim(new Claim("DisplayName", user.DisplayName ?? ""));

            var parish = await _dbContext.Parishes.FindAsync(user.ParishId);
            identity.AddClaim(new Claim("ParishUniqueId", parish?.UniqueId.ToString() ?? string.Empty));

            return identity;
        }
    }

}
