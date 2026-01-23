using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SOK.Application.Common.DTO;
using SOK.Application.Common.Helpers;
using SOK.Application.Common.Interface;
using SOK.Application.Services.Interface;
using SOK.Domain.Entities.Central;
using SOK.Domain.Entities.Parish;
using SOK.Domain.Enums;
using SOK.Infrastructure.Persistence.Context;
using System.Security.Cryptography;

namespace SOK.Infrastructure.Provisioning
{
    /// <summary>
    /// Implementacja provisioningu baz danych dla parafii.
    /// Umożliwia tworzenie baz danych, użytkowników i migracji dla każdej
    /// z zarejestrowanych w centralnej bazie danych parafii.
    /// </summary>
    public class ParishProvisioningService : IParishProvisioningService
    {
        private readonly IConfiguration _cfg;
        private readonly CentralDbContext _central;
        private readonly IServiceProvider _services;
        private readonly ICryptoService _crypto;

        public ParishProvisioningService(IConfiguration config, CentralDbContext centralDbContext,
                                   IServiceProvider serviceProvider, ICryptoService cryptoService)
        {
            _cfg = config;
            _central = centralDbContext;
            _services = serviceProvider;
            _crypto = cryptoService;
        }

        /// <inheritdoc/>
        public async Task<ParishEntry> CreateParishAsync(string parishUid, string parishName, bool? createAdmin = null, bool? seedExampleData = null)
        {
            // Ustal wartości domyślne z konfiguracji, jeśli nie podano
            bool shouldCreateAdmin = createAdmin ?? _cfg.GetValue<bool>("Admin:CreateParishAdmin", true);
            bool shouldSeedData = seedExampleData ?? _cfg.GetValue<bool>("Admin:SeedExampleData", false);

            // Sprawdź czy parafia już istnieje
            var entry = await _central.Parishes.SingleOrDefaultAsync(p => p.UniqueId.ToString() == parishUid);
            if (entry != null)
            {
                throw new InvalidOperationException($"Parish with UID {parishUid} already exists.");
            }

            // Generuj unikalne nazwy bazy danych i użytkownika oraz silne hasło
            string dbName = "Parish_" + RandomNumberGenerator.GetHexString(16);
            string userName = "Tenant_" + RandomNumberGenerator.GetHexString(16);
            string userPwd = RandomNumberGenerator.GetString("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvxyz0123456789!@#$%", 16);

            // Utwórz connection string na podstawie connection stringa administratora
            var adminCs = _cfg["Admin:SqlServer"] ?? throw new InvalidOperationException("Admin connection string not found.");
            var csBuilder = new SqlConnectionStringBuilder(adminCs)
            {
                InitialCatalog = dbName,
                UserID = userName,
                Password = userPwd
            };
            string cs = csBuilder.ToString();

            // Utwórz bazę danych i użytkownika
            await EnsureParishDatabaseReadyAsync(cs);

            // Zapisz do centralnej bazy danych
            var parish = new ParishEntry
            {
                UniqueId = Guid.Parse(parishUid),
                ParishName = parishName,
                EncryptedConnectionString = _crypto.Encrypt(cs)
            };
            _central.Parishes.Add(parish);
            await _central.SaveChangesAsync();

            // Opcjonalnie utwórz użytkownika administratora dla tej parafii
            string? adminUserId = null;
            if (shouldCreateAdmin)
            {
                adminUserId = await CreateParishAdminAsync(parish);
            }

            // Opcjonalnie załaduj przykładowe dane
            if (shouldSeedData)
            {
                using IServiceScope scope = _services.CreateScope();
                IDbSeeder seeder = scope.ServiceProvider.GetRequiredService<IDbSeeder>();
                await seeder.SeedParishDbAsync(
                    parish.UniqueId.ToString(), 
                    adminUserId ?? string.Empty, 
                    seedExampleData: true);
            }

            parish = await _central.Parishes
                .AsNoTracking()
                .Include(p => p.Users)
                .SingleAsync(p => p.UniqueId.ToString() == parishUid);

            return parish;
        }

        /// <inheritdoc/>
        public async Task EnsureAllParishDatabasesReadyAsync()
        {
            var parishes = await _central.Parishes.AsNoTracking().ToListAsync();

            foreach (var parish in parishes)
            {
                var plainCs = _crypto.Decrypt(parish.EncryptedConnectionString);
                await EnsureParishDatabaseReadyAsync(plainCs);
            }
        }

        /// <summary>
        /// Sprawdza, czy baza danych i użytkownik z podanego <c>ConnectionString</c> istnieją
        /// i czy baza ma zastosowane wszystkie migracje. Jeśli nie, to adekwatnie tworzy bazę,
        /// użytkownika i/lub wykonuje migracje.
        /// </summary>
        /// <param name="connectionString"><c>ConnectionString</c> użytkownika danej bazy.</param>
        /// <returns></returns>
        private async Task EnsureParishDatabaseReadyAsync(string connectionString)
        {
            var adminBase = _cfg["Admin:SqlServer"]!;

            // Pobierz potrzebne dane z connection stringa
            var userCsBuilder = new SqlConnectionStringBuilder(connectionString);
            string dbName = userCsBuilder.InitialCatalog;
            string userName = userCsBuilder.UserID;
            string userPwd = userCsBuilder.Password;

            // Utwórz bazę i użytkownika, jeśli nie istnieją
            await using (var adminConn = new SqlConnection(adminBase))
            {
                await adminConn.OpenAsync();
                await CreateDatabase(adminConn, dbName);
                await CreateLogin(adminConn, userName, userPwd);
                await CreateUser(adminConn, dbName, userName);
            }

            // Utwórz connection string administratora do danej bazy
            // (na podstawie jego connection stringa do bazy master)
            var targetCsBuilder = new SqlConnectionStringBuilder(adminBase) { InitialCatalog = dbName };
            string adminCs = targetCsBuilder.ToString();

            // Utwórz kontekst indywidualnej bazy
            using IServiceScope scope = _services.CreateScope();
            ParishDbContext context = scope.ServiceProvider.GetRequiredService<ParishDbContext>();
            context.OverrideConnectionString = adminCs;

            // Używając kontekstu dostępu do bazy danych indywidualnej parafii
            // z konta administratora sprawdź i wykonaj migracje
            if (context.Database.GetPendingMigrations().Any())
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"Applying migrations for database {dbName}...");
                Console.ResetColor();
                await context.Database.MigrateAsync();
            }
        }

        /// <summary>
        /// Tworzy bazę danych o nazwie <paramref name="dbName"/>, jeśli nie istnieje.
        /// </summary>
        /// <param name="connection">Obiekt <see cref="SqlConnection"/>, reprezentujący 
        ///                          połączenie z serwerem przez konto z odpowiednimi uprawnieniami.</param>
        /// <param name="dbName">Nazwa bazy danych.</param>
        /// <returns>
        /// Obiekt <see cref="Task"/>, reprezentujący asynchroniczną operację.
        /// </returns>
        private static async Task CreateDatabase(SqlConnection connection, string dbName)
        {
            // 1) CREATE DATABASE
            // Sprawdzenie, czy baza danych już istnieje oraz jej utworzenie, jeśli nie istnieje.
            await using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = $@"
                        IF DB_ID(@db) IS NULL
                        BEGIN
                            DECLARE @sql NVARCHAR(MAX) = 
                                'CREATE DATABASE ' + quotename(@db) + ';';
                            EXEC(@sql);
                        END";
                //EXEC('CREATE DATABASE quotename({dbName})');

                cmd.Parameters.AddWithValue("@db", dbName);
                await cmd.ExecuteNonQueryAsync();
            }
        }

        /// <summary>
        /// Tworzy login na poziomie serwera o nazwie <paramref name="userName"/> 
        /// i haśle <paramref name="userPwd"/>, jeśli nie istnieje użytkownik o tej nazwie.
        /// </summary>
        /// <param name="connection">Obiekt <see cref="SqlConnection"/>, reprezentujący 
        ///                          połączenie z serwerem przez konto z odpowiednimi uprawnieniami.</param>
        /// <param name="userName">Nazwa użytkownika docelowego.</param>
        /// <param name="userPwd">Hasło użytkownika docelowego.</param>
        /// <returns>
        /// Obiekt <see cref="Task"/>, reprezentujący asynchroniczną operację.
        /// </returns>
        private static async Task CreateLogin(SqlConnection connection, string userName, string userPwd)
        {
            // 2) CREATE LOGIN (server level)
            // Sprawdzenie, czy login już istnieje oraz jego utworzenie, jeśli nie istnieje.
            await using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = $@"
                        IF NOT EXISTS (SELECT 1 FROM sys.sql_logins WHERE name = @login)
                        BEGIN
                            DECLARE @sql NVARCHAR(MAX) = 
                                'CREATE LOGIN ' + quotename(@login) + ' WITH PASSWORD = ' + quotename(@pwd, '''') + ' , CHECK_POLICY = OFF';
                            EXEC(@sql);
                        END";
                //CREATE LOGIN @login WITH PASSWORD = '@pwd', CHECK_POLICY = OFF;

                cmd.Parameters.AddWithValue("@login", userName);
                cmd.Parameters.AddWithValue("@pwd", userPwd);
                await cmd.ExecuteNonQueryAsync();
            }
        }

        /// <summary>
        /// Tworzy użytkownika w danej bazie danych o nazwie <paramref name="dbName"/>, jeśli nie istnieje,
        /// o loginie odpowiadającym jego nazwie (<paramref name="userName"/>). 
        /// Przypisuje mu role <c>db_datareader</c> i <c>db_datawriter</c>.
        /// </summary>
        /// <param name="connection">Obiekt <see cref="SqlConnection"/>, reprezentujący 
        ///                          połączenie z serwerem przez konto z odpowiednimi uprawnieniami.</param>
        /// <param name="dbName">Nazwa bazy danych, do której dostęp ma mieć użytkownik.</param>
        /// <param name="userName">Nazwa użytkownika. Musi istnieć login o tej samej nazwie.</param>
        /// <returns>
        /// Obiekt <see cref="Task"/>, reprezentujący asynchroniczną operację.
        /// </returns
        private static async Task CreateUser(SqlConnection connection, string dbName, string userName)
        {
            // WIP: Sprawdź czy istnieje login o podanej nazwie
            // Jeśli nie, to rzuć wyjątek

            // WIP: Sprawdź czy istnieje baza o podanej nazwie
            // Jeśli nie, to rzuć wyjątek

            // WIP: Jeśli użytkownik istnieje, to sprawdź czy ma przypisane role
            // Jeśli nie, to je przypisz

            // 3) CREATE USER IN DB + role
            // Sprawdzenie, czy użytkownik już istnieje w bazie danych oraz jego utworzenie, jeśli nie istnieje.
            await using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = $@"
                        USE [{dbName}];
                        IF NOT EXISTS (SELECT 1 FROM sys.database_principals WHERE name = @user)
                        BEGIN
                            DECLARE @sql NVARCHAR(MAX) = 
                                'CREATE USER ' + quotename(@user) + ' FOR LOGIN ' + quotename(@user) + ';' +
                                'ALTER ROLE db_datareader ADD MEMBER ' + quotename(@user) + ';' +
                                'ALTER ROLE db_datawriter ADD MEMBER ' + quotename(@user) + ';';
                            EXEC(@sql);
                        END";
                //CREATE USER @user FOR LOGIN @user;
                //ALTER ROLE db_datareader ADD MEMBER @user;
                //ALTER ROLE db_datawriter ADD MEMBER @user;

                cmd.Parameters.AddWithValue("@db", dbName);
                cmd.Parameters.AddWithValue("@user", userName);
                await cmd.ExecuteNonQueryAsync();
            }
        }

        /// <summary>
        /// Zwraca nazwę serwera na podstawie <c>ConnectionString</c> (<paramref name="connectionString"/>).
        /// </summary>
        /// <param name="connectionString">Gotowy <c>ConnectionString</c> do bazy danych.</param>
        /// <returns>
        /// Nazwa serwera, który wskazuje <paramref name="connectionString"/>.
        /// </returns>
        private static string GetServerFromConnectionString(string connectionString)
            => new SqlConnectionStringBuilder(connectionString).DataSource;

        /// <summary>
        /// Tworzy użytkownika administratora dla nowo utworzonej parafii.
        /// Generuje login na podstawie nazwy parafii, tworzy konto centralne (User) oraz
        /// odpowiadający mu wpis w parafialnej bazie (ParishMember).
        /// </summary>
        /// <param name="parish">Parafia, dla której tworzone jest konto administratora.</param>
        /// <returns>
        /// Obiekt <see cref="Task"/>, reprezentujący asynchroniczną operację,
        /// której wartością jest Id utworzonego użytkownika.
        /// </returns>
        private async Task<string> CreateParishAdminAsync(ParishEntry parish)
        {
            using var scope = _services.CreateScope();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
            var currentParishService = scope.ServiceProvider.GetRequiredService<ICurrentParishService>();
            
            // Ustaw kontekst parafii
            await currentParishService.SetParishAsync(parish.UniqueId.ToString());

            // Generuj login i nazwę wyświetlaną
            string baseUserName = parish.ParishName
                .Replace(" ", string.Empty)
                .Replace(",", string.Empty)
                .Replace(".", string.Empty)
                .NormalizePolishDiacritics()
                .ToLower();
            
            string userName = $"{baseUserName}-admin";
            int suffix = 1;
            
            while (await userManager.FindByNameAsync(userName) != null)
            {
                userName = $"{baseUserName}-admin{suffix}";
                suffix++;
            }

            // Generuj silne hasło
            string password = RandomNumberGenerator.GetString("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvxyz0123456789!@#$%", 16);

            // Utwórz użytkownika centralnego
            var adminUser = new User
            {
                UserName = userName,
                DisplayName = $"Administrator",
                ParishId = parish.Id
            };

            var createResult = await userManager.CreateAsync(adminUser, password);

            if (!createResult.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Failed to create admin user for parish {parish.ParishName}: " +
                    string.Join(", ", createResult.Errors.Select(e => e.Description)));
            }

            // Przypisz rolę administratora
            await userManager.AddToRoleAsync(adminUser, Role.Administrator);

            // Utwórz ParishMember w bazie parafialnej
            var parishDbContext = scope.ServiceProvider.GetRequiredService<ParishDbContext>();
            var parishMember = new ParishMember
            {
                CentralUserId = adminUser.Id,
                DisplayName = adminUser.DisplayName
            };

            parishDbContext.Members.Add(parishMember);
            await parishDbContext.SaveChangesAsync();

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"✓ Created admin user for parish '{parish.ParishName}'");
            Console.WriteLine($"  Username: {userName}");
            Console.WriteLine($"  Password: {password}");
            Console.ResetColor();

            return adminUser.Id;
        }
    }
}
