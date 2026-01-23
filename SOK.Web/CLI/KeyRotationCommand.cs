using Microsoft.EntityFrameworkCore;
using SOK.Application.Services.Interface;
using SOK.Infrastructure.Persistence.Context;

namespace SOK.Web.CLI
{
    /// <summary>
    /// Komenda CLI do zarządzania rotacją kluczy szyfrowania.
    /// Umożliwia migrację zaszyfrowanych connection stringów do nowego klucza.
    /// </summary>
    public class KeyRotationCommand
    {
        private readonly CentralDbContext _centralDb;
        private readonly ICryptoService _cryptoService;
        private readonly ILogger<KeyRotationCommand> _logger;

        public KeyRotationCommand(
            CentralDbContext centralDb,
            ICryptoService cryptoService,
            ILogger<KeyRotationCommand> logger)
        {
            _centralDb = centralDb;
            _cryptoService = cryptoService;
            _logger = logger;
        }

        /// <summary>
        /// Wykonuje re-enkrypcję wszystkich connection stringów do nowej wersji klucza.
        /// </summary>
        /// <param name="targetKeyVersion">Docelowa wersja klucza. Jeśli null, użyje aktualnej wersji.</param>
        /// <param name="dryRun">Jeśli true, tylko symuluje operację bez zapisu do bazy.</param>
        public async Task<int> RotateKeysAsync(int? targetKeyVersion = null, bool dryRun = false)
        {
            int targetVersion = targetKeyVersion ?? _cryptoService.GetCurrentKeyVersion();
            
            _logger.LogInformation("=== Key Rotation Command ===");
            _logger.LogInformation($"Target key version: {targetVersion}");
            _logger.LogInformation($"Dry run mode: {dryRun}");
            _logger.LogInformation("");

            var parishes = await _centralDb.Parishes.ToListAsync();
            int totalParishes = parishes.Count;
            int updatedCount = 0;
            int skippedCount = 0;
            int errorCount = 0;

            _logger.LogInformation($"Found {totalParishes} parishes in database");
            _logger.LogInformation("");

            foreach (var parish in parishes)
            {
                try
                {
                    // Sprawdź czy parafia już używa docelowej wersji klucza
                    if (parish.KeyVersion == targetVersion)
                    {
                        _logger.LogInformation($"[SKIP] {parish.ParishName} (UID: {parish.UniqueId}) - already using key v{targetVersion}");
                        skippedCount++;
                        continue;
                    }

                    _logger.LogInformation($"[PROCESSING] {parish.ParishName} (UID: {parish.UniqueId})");
                    _logger.LogInformation($"  Current key version: v{parish.KeyVersion} → Target: v{targetVersion}");

                    // Re-enkryptuj connection string
                    string reencrypted = _cryptoService.Reencrypt(parish.EncryptedConnectionString, targetVersion);

                    if (!dryRun)
                    {
                        parish.EncryptedConnectionString = reencrypted;
                        parish.KeyVersion = targetVersion;
                        updatedCount++;
                        _logger.LogInformation($"  ✓ Updated successfully");
                    }
                    else
                    {
                        _logger.LogInformation($"  ✓ Would update (dry run)");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"  ✗ Error: {ex.Message}");
                    errorCount++;
                }

                _logger.LogInformation("");
            }

            if (!dryRun && updatedCount > 0)
            {
                await _centralDb.SaveChangesAsync();
                _logger.LogInformation($"Changes saved to database");
            }

            _logger.LogInformation("=== Summary ===");
            _logger.LogInformation($"Total parishes: {totalParishes}");
            _logger.LogInformation($"Updated: {updatedCount}");
            _logger.LogInformation($"Skipped: {skippedCount}");
            _logger.LogInformation($"Errors: {errorCount}");

            return errorCount > 0 ? 1 : 0;
        }

        /// <summary>
        /// Wyświetla raport z wersjami kluczy używanych przez parafie.
        /// </summary>
        public async Task ShowKeyVersionsReportAsync()
        {
            _logger.LogInformation("=== Key Versions Report ===");
            _logger.LogInformation($"Current system key version: v{_cryptoService.GetCurrentKeyVersion()}");
            _logger.LogInformation("");

            var parishes = await _centralDb.Parishes
                .OrderBy(p => p.KeyVersion)
                .ThenBy(p => p.ParishName)
                .ToListAsync();

            var groupedByVersion = parishes.GroupBy(p => p.KeyVersion);

            foreach (var group in groupedByVersion)
            {
                _logger.LogInformation($"Key Version v{group.Key}: {group.Count()} parishes");
                foreach (var parish in group)
                {
                    _logger.LogInformation($"  - {parish.ParishName} (UID: {parish.UniqueId})");
                }
                _logger.LogInformation("");
            }

            _logger.LogInformation($"Total parishes: {parishes.Count}");
        }
    }
}
