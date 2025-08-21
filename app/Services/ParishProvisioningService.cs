using app.Data;
using app.Models.Central;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace app.Services
{
    /// <summary>
    /// Interfejs do provisioningu indywidualnych baz danych dla parafii.
    /// </summary>
    public interface IProvisioningService
    {
        Task ProvisionParishAsync(string parishUid, string dbUser, string dbPassword);
        Task EnsureAllParishDatabasesMigratedAsync();
    }

    /// <summary>
    /// Implementacja provisioningu baz danych dla parafii.
    /// Umożliwia tworzenie baz danych, użytkowników i migracji dla każdej
    /// z zarejestrowanych w centralnej bazie danych parafii.
    /// </summary>
    public class ProvisioningService : IProvisioningService
    {
        private readonly IConfiguration _cfg;
        private readonly CentralDbContext _central;
        private readonly IParishDbContextFactory _parishFactory;
        private readonly ICryptoService _crypto;

        public ProvisioningService(IConfiguration config, CentralDbContext centralDbContext,
                                   IParishDbContextFactory parishDbContextFactory, ICryptoService cryptoService)
        {
            _cfg = config; _central = centralDbContext; _parishFactory = parishDbContextFactory; _crypto = cryptoService;
        }

        /// <summary>
        /// Sprawdza, czy baza danych dla danej parafii 
        /// istnieje i jest poprawnie skonfigurowana.
        /// </summary>
        /// <param name="parishUid">Identyfikator UID parafii</param>
        /// <param name="dbUser">Nazwa użytkownika bazy danych</param>
        /// <param name="dbPassword">Hasło użytkownika bazy danych</param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public async Task ProvisionParishAsync(string parishUid, string dbUser, string dbPassword)
        {
            var adminCs = _cfg["Admin:SqlServer"] ?? throw new InvalidOperationException("Admin connection string not found.");
            var dbName = $"Parish_{parishUid}";
            var login = $"tenant_{parishUid}";

            using (var conn = new SqlConnection(adminCs))
            {
                await conn.OpenAsync();

                // 1) CREATE DATABASE
                // Sprawdzenie, czy baza danych już istnieje oraz jej utworzenie, jeśli nie istnieje.
                await using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = $@"
                        IF DB_ID(@db) IS NULL
                        BEGIN
                            EXEC('CREATE DATABASE [{dbName}]');
                        END";

                    cmd.Parameters.AddWithValue("@db", dbName);
                    await cmd.ExecuteNonQueryAsync();
                }

                // 2) CREATE LOGIN (server level)
                // Sprawdzenie, czy login już istnieje oraz jego utworzenie, jeśli nie istnieje.
                await using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = $@"
                        IF NOT EXISTS (SELECT 1 FROM sys.sql_logins WHERE name = @login)
                        BEGIN
                            CREATE LOGIN [{login}] WITH PASSWORD = @pwd, CHECK_POLICY = OFF;
                        END";

                    cmd.Parameters.AddWithValue("@login", login);
                    cmd.Parameters.AddWithValue("@pwd", dbPassword);
                    await cmd.ExecuteNonQueryAsync();
                }

                // 3) CREATE USER IN DB + role
                // Sprawdzenie, czy użytkownik już istnieje w bazie danych oraz jego utworzenie, jeśli nie istnieje.
                await using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = $@"
                        USE [{dbName}];
                        IF NOT EXISTS (SELECT 1 FROM sys.database_principals WHERE name = @user)
                        BEGIN
                            CREATE USER [{login}] FOR LOGIN [{login}];
                        END
                        EXEC sp_addrolemember 'db_datareader/db_datawriter', @user;";

                    cmd.Parameters.AddWithValue("@user", login);
                    await cmd.ExecuteNonQueryAsync();
                }
            }

            // 4) Connection string dla aplikacji
            var tenantCs = $"Server={GetServerFromConnectionString(adminCs)};Database={dbName};User Id={login};Password={dbPassword};TrustServerCertificate=True";
            var encCs = _crypto.Encrypt(tenantCs);

            // 5) Zapis do centralnej bazy danych
            var entry = await _central.Parishes.SingleOrDefaultAsync(p => p.UniqueId.ToString() == parishUid);
            if (entry is null)
            {
                _central.Parishes.Add(new ParishEntry 
                    { 
                        UniqueId = Guid.Parse(parishUid), 
                        ParishName = parishUid, 
                        EncryptedConnectionString = encCs 
                    });
            }
            else
            {
                entry.EncryptedConnectionString = encCs;
            }
            await _central.SaveChangesAsync();

            // 6) Migracje
            await ApplyParishMigrationsAsync(dbName);
        }

        /// <summary>
        /// Sprawdza, czy wszystkie parafialne bazy mają aktualne migracje i stosuje je.
        /// </summary>
        /// <returns></returns>
        public async Task EnsureAllParishDatabasesMigratedAsync()
        {
            var adminCs = _cfg["Admin:SqlServer"]!;
            
            var all = await _central.Parishes.AsNoTracking().ToListAsync();
            foreach (var p in all)
            {
                var dbName = $"Parish_{p.UniqueId}";
                await ApplyParishMigrationsAsync(dbName);
            }
        }

        /// <summary>
        /// Wykonuje migracje dla danej bazy danych parafii jako administrator.
        /// </summary>
        /// <param name="dbName">Nazwa bazy danych parafii</param>
        /// <returns></returns>
        private async Task ApplyParishMigrationsAsync(string dbName)
        {
            var adminBase = _cfg["Admin:SqlServer"]!;

            // Zamień Database=master -> Database=Parish_xxx
            var builder = new SqlConnectionStringBuilder(adminBase) { InitialCatalog = dbName };
            var cs = builder.ToString();

            using var ctx = _parishFactory.Create(cs);
            await ctx.Database.MigrateAsync();
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
