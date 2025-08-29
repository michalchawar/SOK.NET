using app.Data;
using app.Models.Central;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;
using NuGet.Protocol.Plugins;
using System.Security.Cryptography;
using System.Xml.Linq;

namespace app.Services.Parish
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

        public async Task<ParishEntry> CreateParishAsync(string parishUid, string parishName)
        {
            // Sprawdź czy parafia już istnieje
            var entry = await _central.Parishes.SingleOrDefaultAsync(p => p.UniqueId.ToString() == parishUid);
            if (entry != null)
            {
                throw new InvalidOperationException($"Parish with UID {parishUid} already exists.");
            }

            // Generuj unikalne nazwy bazy danych i użytkownika oraz silne hasło
            string dbName   = "Parish_" + RandomNumberGenerator.GetHexString(8);
            string userName = "tenant_" + RandomNumberGenerator.GetHexString(8);
            string userPwd  = RandomNumberGenerator.GetString("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvxyz0123456789!@#$%", 16);

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

            return parish;
        }

        /// <summary>
        /// Sprawdza, czy wszystkie parafialne bazy istnieją i mają aktualne migracje.
        /// Jeśli nie, to je tworzy i/lub wykonuje migracje.
        /// </summary>
        /// <returns></returns>
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
        /// Sprawdza, czy baza danych i użytkownik z podanego ConnectionStringa istnieją
        /// i czy baza ma zastosowane wszystkie migracje. Jeśli nie, to adekwatnie tworzy bazę,
        /// użytkownika i/lub wykonuje migracje.
        /// </summary>
        /// <param name="connectionString">ConnectionString użytkownika danej bazy</param>
        /// <returns></returns>
        private async Task EnsureParishDatabaseReadyAsync(string connectionString)
        {
            var adminBase = _cfg["Admin:SqlServer"]!;

            // Pobierz potrzebne dane z connection stringa
            var userCsBuilder   = new SqlConnectionStringBuilder(connectionString);
            string dbName   = userCsBuilder.InitialCatalog;
            string userName = userCsBuilder.UserID;
            string userPwd  = userCsBuilder.Password;

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
            context.Database.SetConnectionString(adminCs);

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
        /// Tworzy bazę danych o podanej nazwie, jeśli nie istnieje.
        /// </summary>
        /// <param name="connection">Obiekt SqlConnection, reprezentujący 
        ///                          połączenie z serwerem przez konto z odpowiednimi uprawnieniami</param>
        /// <param name="dbName">Nazwa bazy danych</param>
        /// <returns></returns>
        private static async Task CreateDatabase(SqlConnection connection, string dbName)
        {
            // 1) CREATE DATABASE
            // Sprawdzenie, czy baza danych już istnieje oraz jej utworzenie, jeśli nie istnieje.
            await using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = $@"
                        IF DB_ID(@db) IS NULL
                        BEGIN
                            EXEC('CREATE DATABASE [{dbName}]');
                        END";

                cmd.Parameters.AddWithValue("@db", dbName);
                await cmd.ExecuteNonQueryAsync();
            }
        }

        /// <summary>
        /// Tworzy login na poziomie serwera o podanej nazwie i haśle, jeśli nie istnieje.
        /// </summary>
        /// <param name="connection">Obiekt SqlConnection, reprezentujący 
        ///                          połączenie z serwerem przez konto z odpowiednimi uprawnieniami</param>
        /// <param name="userName">Nazwa użytkownika docelowego</param>
        /// <param name="userPwd">Hasło użytkownika docelowego</param>
        /// <returns></returns>
        private static async Task CreateLogin(SqlConnection connection, string userName, string userPwd)
        {
            // 2) CREATE LOGIN (server level)
            // Sprawdzenie, czy login już istnieje oraz jego utworzenie, jeśli nie istnieje.
            await using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = $@"
                        IF NOT EXISTS (SELECT 1 FROM sys.sql_logins WHERE name = @login)
                        BEGIN
                            CREATE LOGIN [{userName}] WITH PASSWORD = @pwd, CHECK_POLICY = OFF;
                        END";

                cmd.Parameters.AddWithValue("@login", userName);
                cmd.Parameters.AddWithValue("@pwd", userPwd);
                await cmd.ExecuteNonQueryAsync();
            }
        }

        /// <summary>
        /// Tworzy użytkownika w danej bazie danych o podanej nazwie, jeśli nie istnieje,
        /// o loginie odpowiadającym jego nazwie. Przypisuje mu role db_datareader i db_datawriter.
        /// </summary>
        /// <param name="connection">Obiekt SqlConnection, reprezentujący 
        ///                          połączenie z serwerem przez konto z odpowiednimi uprawnieniami</param>
        /// <param name="dbName">Nazwa bazy danych, do której dostęp ma mieć użytkownik</param>
        /// <param name="userName">Nazwa użytkownika. Musi istnieć login o tej samej nazwie.</param>
        /// <returns></returns
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
                            CREATE USER [{userName}] FOR LOGIN [{userName}];
                            ALTER ROLE db_datareader ADD MEMBER [{userName}];
                            ALTER ROLE db_datawriter ADD MEMBER [{userName}];
                        END";

                cmd.Parameters.AddWithValue("@user", userName);
                await cmd.ExecuteNonQueryAsync();
            }
        }

        /// <summary>
        /// Zwraca nazwę serwera na podstawie connection stringa.
        /// </summary>
        /// <param name="cs">Gotowy ConnectionString</param>
        /// <returns></returns>
        private static string GetServerFromConnectionString(string cs)
            => new SqlConnectionStringBuilder(cs).DataSource;
    }

}
