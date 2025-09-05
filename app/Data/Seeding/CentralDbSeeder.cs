using app.Models.Central.Entities;
using app.Models.Central.Enums;
using app.Extensions;
using app.Services;
using app.Services.Parish;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace app.Data.Seeding
{
    public static class CentralDbSeeder
    {
        /// <summary>
        /// Tworzy początkowe dane w bazie centralnej.
        /// </summary>
        /// <param name="context">Kontekst dostępu do bazy danych.</param>
        /// <param name="userManager">Menedżer użytkowników.</param>
        /// <param name="roleManager">Menedżer ról.</param>
        /// <param name="parishProvisioning">Serwis do provisioningu indywidualnych baz danych parafii.</param>
        /// <returns>
        /// Obiekt <see cref="Task"/>, reprezentujący operację asynchroniczną.
        /// </returns>
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
        public static async Task SeedAsync(
            CentralDbContext context,
            UserManager<User> userManager,
            RoleManager<IdentityRole> roleManager,
            IParishProvisioningService parishProvisioning)
        {
            // 1. Sprawdź i wykonaj migracje, jeśli oczekują
            if (context.Database.GetPendingMigrations().Any())
            {
                Console.ForegroundColor = ConsoleColor.DarkCyan;
                Console.WriteLine("Applying migrations for the central database...");
                Console.ResetColor();
                context.Database.Migrate();
            }

            // 2. Utwórz role jeśli nie istnieją
            foreach (string role in Enum.GetNames(typeof(Role)))
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // 3. Upewnij się, że istnieje parafia
            var parish = await context.Parishes.FirstOrDefaultAsync();
            if (parish == null)
            {
                parish = await parishProvisioning.CreateParishAsync(Guid.NewGuid().ToString(), "Przykładowa parafia");
            }

            // 4. Upewnij się, że istnieje administrator
            var adminUserName = "admin";
            var adminUser = await userManager.FindByNameAsync(adminUserName);

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

                var result = await userManager.CreateAsync(adminUser, "admin");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, Role.Administrator);
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
