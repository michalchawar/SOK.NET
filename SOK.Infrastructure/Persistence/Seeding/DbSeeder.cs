using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SOK.Application.Common.DTO;
using SOK.Application.Common.Helpers;
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
        private readonly IConfiguration _configuration;

        public DbSeeder(
            CentralDbContext context,
            UserManager<Domain.Entities.Central.User> userManager,
            RoleManager<IdentityRole> roleManager,
            IParishProvisioningService parishProvisioning,
            IServiceProvider serviceProvider,
            IConfiguration configuration)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _parishProvisioning = parishProvisioning;
            _serviceProvider = serviceProvider;
            _configuration = configuration;
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
                parish = await _parishProvisioning.CreateParishAsync(
                    Guid.NewGuid().ToString(), 
                    "Przykładowa parafia",
                    createAdmin: true,
                    seedExampleData: _configuration.GetValue<bool>("Admin:SeedExampleData", false)
                );
            }

            // 4. Przygotuj konto superadministratora
            string adminUserName = string.IsNullOrWhiteSpace(_configuration["Admin:Username"]) ? "admin" : _configuration["Admin:Username"]!;
            string adminPassword = string.IsNullOrWhiteSpace(_configuration["Admin:Password"]) ? "admin" : _configuration["Admin:Password"]!;
            string adminEmail = string.IsNullOrWhiteSpace(_configuration["Admin:Email"]) ? $"{adminUserName}@system.local" : _configuration["Admin:Email"]!;
            string adminDisplayName = string.IsNullOrWhiteSpace(_configuration["Admin:DisplayName"]) ? "System Administrator" : _configuration["Admin:DisplayName"]!;
            
            // 5. Jeśli jest taka potrzeba, utwórz je
            var adminUser = await _userManager.FindByNameAsync(adminUserName);
            bool shouldAdminBeCreated = adminUser is null;

            if (shouldAdminBeCreated) {
                adminUser = new Domain.Entities.Central.User
                {
                    UserName = adminUserName,
                    Email = adminEmail,
                    DisplayName = adminDisplayName,
                    EmailConfirmed = true
                };

                var result = await _userManager.CreateAsync(adminUser, adminPassword!);
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(adminUser, Role.SuperAdmin);
                }
                else
                {
                    throw new Exception("Nie udało się utworzyć administratora: " +
                        string.Join(", ", result.Errors.Select(e => e.Description)));
                }
            }
        }

        /// <inheritdoc />
        public async Task SeedParishDbAsync(string parishUid, string adminId, bool seedExampleData = false)
        {
            Console.WriteLine("In SeedParishDbAsync");

            try
            {
                using IServiceScope scope = _serviceProvider.CreateScope();
                ICurrentParishService current = scope.ServiceProvider.GetRequiredService<ICurrentParishService>();

                await current.SetParishAsync(parishUid);

                ParishDbContext context = scope.ServiceProvider.GetRequiredService<ParishDbContext>();
                ISubmissionService submissionService = scope.ServiceProvider.GetRequiredService<ISubmissionService>();

                var admin = await context.Members.FirstOrDefaultAsync(m => m.CentralUserId == adminId);

                await using var transaction = await context.Database.BeginTransactionAsync();

                if (seedExampleData == false)
                {
                    context.Add(new ParishInfo { Name = InfoKeys.Parish.UniqueId, Value = parishUid });

                    StreetSpecifier 
                        sp_1 = new StreetSpecifier { FullName = "aleja", Abbreviation = "al." },
                        sp_2 = new StreetSpecifier { FullName = "bulwar", Abbreviation = "bulwar" },
                        sp_3 = new StreetSpecifier { FullName = "plac", Abbreviation = "pl." },
                        sp_4 = new StreetSpecifier { FullName = "ulica", Abbreviation = "ul." };
                    context.AddRange([sp_1, sp_2, sp_3, sp_4]);
                    await context.SaveChangesAsync();
                    await transaction.CommitAsync();
                    return;
                }

                City 
                    city = new City { Name = "Miasto", DisplayName = "Miasto" };
                context.Add(city);

                StreetSpecifier 
                    sp1 = new StreetSpecifier { FullName = "aleja", Abbreviation = "al." },
                    sp2 = new StreetSpecifier { FullName = "bulwar", Abbreviation = "bulwar" },
                    sp3 = new StreetSpecifier { FullName = "plac", Abbreviation = "pl." },
                    sp4 = new StreetSpecifier { FullName = "ulica", Abbreviation = "ul." };
                context.AddRange([sp1, sp2, sp3, sp4]);

                Street 
                    street1 = new Street { City = city, Type = sp4, Name = "Pierwsza" },
                    street2 = new Street { City = city, Type = sp4, Name = "Druga" };
                context.AddRange([street1, street2]);

                Building 
                    building1 = new Building { Street = street1, Number = 2, AllowSelection = true },
                    building2 = new Building { Street = street1, Number = 4, AllowSelection = true },
                    building3 = new Building { Street = street1, Number = 6, AllowSelection = true },
                    building4 = new Building { Street = street1, Number = 8, AllowSelection = true },
                    building5 = new Building { Street = street2, Number = 1, AllowSelection = true },
                    building6 = new Building { Street = street2, Number = 2, AllowSelection = true },
                    building7 = new Building { Street = street1, Number = 3, Letter = "a", AllowSelection = true },
                    building8 = new Building { Street = street1, Number = 3, Letter = "b", AllowSelection = true };
                context.AddRange([building1, building2, building3, building4, building5, building6, 
                    building7, building8]);

                Plan plan = new Plan() { Name = "Kolęda 2024/25", Author = admin };
                context.AddRange([plan]);

                Schedule 
                    schedule1 = new Schedule { Name = "Kolęda w terminie", ShortName = "T", Color = "#bec5d1", Plan = plan },
                    schedule2 = new Schedule { Name = "Kolęda dodatkowa",  ShortName = "D", Color = "#3b73d4", Plan = plan };
                context.AddRange([schedule1, schedule2]);

                await context.SaveChangesAsync();

                var submissions = new[] {
                    new SubmissionCreationRequestDto {
                        Submitter = new Submitter { Name = "Jan", Surname = "Kowalski", Email = "jankowalski@test.testtest" },
                        Building = building1,
                        ApartmentNumber = 4,
                        Schedule = schedule1
                    },
                    new SubmissionCreationRequestDto {
                        Submitter = new Submitter { Name = "Adam", Surname = "Sadowski", Email = "adamsad@test.testtest" },
                        Building = building1,
                        ApartmentNumber = 7,
                        Schedule = schedule1
                    },
                    new SubmissionCreationRequestDto {
                        Submitter = new Submitter { Name = "Tymon", Surname = "Michalik" },
                        Building = building2,
                        ApartmentNumber = 13,
                        ApartmentLetter = "b",
                        Schedule = schedule1
                    },
                    new SubmissionCreationRequestDto {
                        Submitter = new Submitter { Name = "Aurelia", Surname = "Żak", Email = "aurweks312@test.testtest" },
                        Building = building2,
                        ApartmentNumber = 2,
                        Schedule = schedule1
                    },
                    new SubmissionCreationRequestDto {
                        Submitter = new Submitter { Name = "Marcel", Surname = "Zieliński", Phone = "546328964" },
                        Building = building3,
                        ApartmentNumber = 5,
                        Schedule = schedule1
                    },
                    new SubmissionCreationRequestDto {
                        Submitter = new Submitter { Name = "Jakub", Surname = "Mazurek", Email = "mazur231@test.testtest" },
                        Building = building5,
                        ApartmentNumber = 3,
                        Schedule = schedule1
                    },
                    new SubmissionCreationRequestDto {
                        Submitter = new Submitter { Name = "Julita i Zdzisław", Surname = "Szczepańscy", Email = "szczep1542@test.testtest" },
                        Building = building5,
                        ApartmentNumber = 6,
                        Schedule = schedule1,
                        Author = admin
                    },
                    new SubmissionCreationRequestDto {
                        Submitter = new Submitter { Name = "Ewa", Surname = "Kowal", Email = "awdszrw13@test.testtest", Phone = "465213782" },
                        Building = building5,
                        ApartmentNumber = 4,
                        Schedule = schedule1
                    },
                    new SubmissionCreationRequestDto {
                        Submitter = new Submitter { Name = "Karol", Surname = "Kasprzak", Email = "karolskrez@test.testtest" },
                        Building = building5,
                        ApartmentNumber = 7,
                        ApartmentLetter = "A",
                        Schedule = schedule1
                    },
                    new SubmissionCreationRequestDto {
                        Submitter = new Submitter { Name = "Karol", Surname = "Kasprzak", Email = "karolskrez@test.testtest" },
                        Building = building6,
                        ApartmentNumber = 9,
                        Schedule = schedule1,
                        Author = admin
                    },
                    new SubmissionCreationRequestDto {
                        Submitter = new Submitter { Name = "Fryderyk", Surname = "Przybylski" },
                        Building = building6,
                        ApartmentNumber = 11,
                        Schedule = schedule1,
                        Author = admin
                    },
                    new SubmissionCreationRequestDto {
                        Submitter = new Submitter { Name = "Apolonia", Surname = "Kosińska", Email = "kosapol@test.testtest", Phone = "694783274" },
                        Building = building7,
                        ApartmentNumber = 1,
                        Schedule = schedule1
                    }
                };

                foreach (var request in submissions)
                {
                    try
                    {
                        await submissionService.CreateSubmissionAsync(request);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error while seeding submissions: {ex.Message}");
                    }
                }

                KeyValuePair<string, string>[] defaultSettings = new[]
                {
                    new KeyValuePair<string, string>(InfoKeys.Parish.FullName, "Parafia Domyślna (nazwa pełna)"),
                    new KeyValuePair<string, string>(InfoKeys.Parish.ShortName, "Parafia Domyślna"),
                    new KeyValuePair<string, string>(InfoKeys.Parish.UniqueId, parishUid),
                    new KeyValuePair<string, string>(InfoKeys.Parish.Diocese, "Archidiecezja Przykładowa"),
                    new KeyValuePair<string, string>(InfoKeys.Contact.CityName, "Miasto"),
                    new KeyValuePair<string, string>(InfoKeys.Contact.StreetAndBuilding, "pl. Kościelny 1"),
                    new KeyValuePair<string, string>(InfoKeys.Contact.PostalCode, "00-000"),
                    new KeyValuePair<string, string>(InfoKeys.Contact.RegionAndCountry, "Województwo losowe, Polska"),
                    new KeyValuePair<string, string>(InfoKeys.Contact.Email, "parafia@domyslna.test"),
                    new KeyValuePair<string, string>(InfoKeys.Contact.MainPhone, "+48 123 456789"),
                    new KeyValuePair<string, string>(InfoKeys.Contact.SecondaryPhone, "+48 987 654 321"),
                };

                foreach (var setting in defaultSettings)
                {
                    context.Add(new ParishInfo { Name = setting.Key, Value = setting.Value });
                }
                await context.SaveChangesAsync();

                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while seeding newly created parish database: {ex.Message}");
            }

            return;
        }
    }
}
