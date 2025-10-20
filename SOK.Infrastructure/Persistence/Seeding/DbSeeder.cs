using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SOK.Application.Common.Interface;
using SOK.Application.Services.Interface;
using SOK.Domain.Entities.Parish;
using SOK.Domain.Enums;
using SOK.Infrastructure.Extensions;
using SOK.Infrastructure.Persistence.Context;
using SOK.Infrastructure.Provisioning;

namespace SOK.Infrastructure.Persistence.Seeding
{
    /// <inheritdoc />
    public class DbSeeder : IDbSeeder
    {
        private readonly CentralDbContext _context;
        private readonly UserManager<Domain.Entities.Central.User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IParishProvisioningService _parishProvisioning;
        private readonly IServiceProvider _serviceProvider;

        public DbSeeder(
            CentralDbContext context,
            UserManager<Domain.Entities.Central.User> userManager,
            RoleManager<IdentityRole> roleManager,
            IParishProvisioningService parishProvisioning,
            IServiceProvider serviceProvider)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _parishProvisioning = parishProvisioning;
            _serviceProvider = serviceProvider;
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
                // Zaludnij bazę danych parafii
                // Można to zrobić zawsze, bo jeśli parafia wcześniej istniała to CreateParishAsync wywali błąd
                await SeedParishDbAsync(parish.UniqueId.ToString());
            }

            // 4. Upewnij się, że istnieje administrator
            var adminUserName = "admin";
            var adminUser = await _userManager.FindByNameAsync(adminUserName);

            if (adminUser == null)
            {
                adminUser = new Domain.Entities.Central.User
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

        private async Task SeedParishDbAsync(string parishUid)
        {
            Console.WriteLine("In SeedParishDbAsyc");

            try
            {
                using IServiceScope scope = _serviceProvider.CreateScope();
                ICurrentParishService current = scope.ServiceProvider.GetRequiredService<ICurrentParishService>();

                await current.SetParishAsync(parishUid);

                ParishDbContext context = scope.ServiceProvider.GetRequiredService<ParishDbContext>();
                
                City 
                    city = new City { Name = "Miasto", DisplayName = "Miasto" };
                context.Add(city);

                StreetSpecifier 
                    sp = new StreetSpecifier { FullName = "ulica", Abbreviation = "ul." };
                context.Add(sp);

                Street 
                    street1 = new Street { City = city, Type = sp, Name = "Pierwsza" },
                    street2 = new Street { City = city, Type = sp, Name = "Druga" };
                context.AddRange([street1, street2]);

                Building 
                    building1 = new Building { Street = street1, Number = 2 },
                    building2 = new Building { Street = street1, Number = 4 },
                    building3 = new Building { Street = street1, Number = 6 },
                    building4 = new Building { Street = street1, Number = 8 },
                    building5 = new Building { Street = street2, Number = 1 },
                    building6 = new Building { Street = street1, Number = 2 },
                    building7 = new Building { Street = street1, Number = 3, Letter = "a" },
                    building8 = new Building { Street = street1, Number = 3, Letter = "b" };
                context.AddRange([building1, building2, building3, building4, building5, building6, 
                    building7, building8]);

                Address 
                    address1 = new Address { Building = building1, ApartmentNumber = 4 },
                    address2 = new Address { Building = building1, ApartmentNumber = 7 },
                    address3 = new Address { Building = building2, ApartmentNumber = 13, ApartmentLetter = "b" },
                    address4 = new Address { Building = building2, ApartmentNumber = 2 },
                    address5 = new Address { Building = building2, ApartmentNumber = 1 },
                    address6 = new Address { Building = building3, ApartmentNumber = 5 },
                    address7 = new Address { Building = building5, ApartmentNumber = 3 },
                    address8 = new Address { Building = building5, ApartmentNumber = 6 },
                    address9 = new Address { Building = building5, ApartmentNumber = 4 },
                   address10 = new Address { Building = building5, ApartmentNumber = 3 },
                   address11 = new Address { Building = building6, ApartmentNumber = 9 },
                   address12 = new Address { Building = building6, ApartmentNumber = 11 },
                   address13 = new Address { Building = building7, ApartmentNumber = 1 },
                   address14 = new Address { Building = building8, ApartmentNumber = 4 };
                context.AddRange([address1, address2, address3, address4, address5, address6, address7,
                    address8, address9, address10, address11, address12, address13, address14]);

                Submitter 
                    submitter1 = new Submitter { Name = "Jan", Surname = "Kowalski", Email = "jankowalski@test.testtest" },
                    submitter2 = new Submitter { Name = "Adam", Surname = "Sadowski", Email = "adamsad@test.testtest" },
                    submitter3 = new Submitter { Name = "Tymon", Surname = "Michalik" },
                    submitter4 = new Submitter { Name = "Aurelia", Surname = "Żak", Email = "aurweks312@test.testtest" },
                    submitter5 = new Submitter { Name = "Marcel", Surname = "Zieliński", Phone = "546328964" },
                    submitter6 = new Submitter { Name = "Jakub", Surname = "Mazurek", Email = "mazur231@test.testtest" },
                    submitter7 = new Submitter { Name = "Julita i Zdzisław", Surname = "Szczepańscy", Email = "szczep1542@test.testtest" },
                    submitter8 = new Submitter { Name = "Ewa", Surname = "Kowal", Email = "awdszrw13@test.testtest", Phone = "465213782" },
                    submitter9 = new Submitter { Name = "Karol", Surname = "Kasprzak", Email = "karolskrez@test.testtest" },
                   submitter10 = new Submitter { Name = "Fryderyk", Surname = "Przybylski" },
                   submitter11 = new Submitter { Name = "Apolonia", Surname = "Kosińska", Email = "kosapol@test.testtest", Phone = "694783274" };
                context.AddRange([submitter1, submitter2, submitter3, submitter4, submitter5, submitter6,
                    submitter7, submitter8, submitter9, submitter10, submitter11]);

                Submission 
                    submission1 = new Submission { Submitter = submitter1, Address = address1 },
                    submission2 = new Submission { Submitter = submitter2, Address = address2 },
                    submission3 = new Submission { Submitter = submitter3, Address = address3 },
                    submission4 = new Submission { Submitter = submitter4, Address = address4 },
                    submission5 = new Submission { Submitter = submitter4, Address = address5 },
                    submission6 = new Submission { Submitter = submitter5, Address = address6 },
                    submission7 = new Submission { Submitter = submitter6, Address = address7 },
                    submission8 = new Submission { Submitter = submitter7, Address = address8 },
                    submission9 = new Submission { Submitter = submitter8, Address = address9 },
                   submission10 = new Submission { Submitter = submitter9, Address = address10 },
                   submission11 = new Submission { Submitter = submitter9, Address = address11 },
                   submission12 = new Submission { Submitter = submitter10, Address = address12 },
                   submission13 = new Submission { Submitter = submitter11, Address = address13 };
                context.AddRange([submission1, submission2, submission3, submission4, submission5, submission6,
                    submission7, submission8, submission9, submission10, submission11, submission12, submission13]);

                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while seeding newly created parish database: {ex.Message}");
            }

            return;
        }
    }
}
