using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SOK.Application.Common.Interface;
using SOK.Domain.Entities.Central;
using SOK.Domain.Enums;
using SOK.Infrastructure.Extensions;
using SOK.Infrastructure.Persistence.Context;
using SOK.Infrastructure.Provisioning;

namespace SOK.Infrastructure.Persistence.Seeding
{
    /// <inheritdoc />
    public class CentralDbSeeder : IDbSeeder
    {
        private readonly CentralDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IParishProvisioningService _parishProvisioning;

        public CentralDbSeeder(
            CentralDbContext context,
            UserManager<User> userManager,
            RoleManager<IdentityRole> roleManager,
            IParishProvisioningService parishProvisioning)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _parishProvisioning = parishProvisioning;
        }

        /// <inheritdoc />
        /// <exception cref="Exception"></exception>
        /// <remarks>
        /// Wykonuje następujące kroki:
        /// <list type="number">
        ///     <item><description>
        ///         Wykonuje migracje bazy danych, jeśli są oczekujące.
        ///     </description></item>
        ///     <item><description>
        ///         Tworzy role na podstawie enuma <see cref="Role"/>.
        ///     </description></item>
        ///     <item><description>
        ///         Tworzy przykładową parafię (jeśli żadnej jeszcze nie ma w bazie).
        ///     </description></item>
        ///     <item><description>
        ///         Tworzy użytkownika administratora o nazwie "admin" i haśle "admin", należącego do przykładowej parafii
        ///         (jeśli jeszcze nie istnieje) i przypisuje mu rolę <see cref="Role.Administrator"/>.
        ///     </description></item>
        /// </list>
        /// 
        /// Jeśli parafia już istnieje, nowa nie jest tworzona, a użytkownik administratora 
        /// jest przypisywany do istniejącej parafii.
        /// </remarks>
        public async Task SeedAsync()
        {
            // 1. Sprawdź i wykonaj migracje, jeśli oczekują
            if (_context.Database.GetPendingMigrations().Any())
            {
                Console.ForegroundColor = ConsoleColor.DarkCyan;
                Console.WriteLine("Applying migrations for the central database...");
                Console.ResetColor();
                _context.Database.Migrate();
            }

            // 2. Utwórz role jeśli nie istnieją
            foreach (string role in Enum.GetNames(typeof(Role)))
            {
                if (!await _roleManager.RoleExistsAsync(role))
                {
                    await _roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // 3. Upewnij się, że istnieje parafia
            var parish = await _context.Parishes.FirstOrDefaultAsync();
            if (parish == null)
            {
                parish = await _parishProvisioning.CreateParishAsync(Guid.NewGuid().ToString(), "Przykładowa parafia");
            }

            // 4. Upewnij się, że istnieje administrator
            var adminUserName = "admin";
            var adminUser = await _userManager.FindByNameAsync(adminUserName);

            if (adminUser == null)
            {
                adminUser = new User
                {
                    UserName = adminUserName,
                    Email = adminUserName + "@system.local",
                    DisplayName = "System Administrator",
                    EmailConfirmed = true,
                    Parish = parish
                };

                var result = await _userManager.CreateAsync(adminUser, "admin");
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(adminUser, Role.Administrator);
                }
                else
                {
                    throw new Exception("Nie udało się utworzyć administratora: " +
                        string.Join(", ", result.Errors.Select(e => e.Description)));
                }
            }
        }

    }
}
