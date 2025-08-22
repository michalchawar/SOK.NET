using app.Models;
using app.Data;
using Microsoft.EntityFrameworkCore;
using app.Services;

namespace app.Services.ParishService
{
    /// <summary>
    /// Implementacja serwisu do ³adowania i przechowywania aktualnie wybranej parafii.
    /// Parafia jest pobierana z centralnej bazy danych na podstawie jej publicznego unikalnego identyfikatora (UID).
    /// </summary>
    public class CurrentParishService : ICurrentParishService
    {
        private readonly CentralDbContext _context;
        
        public string? ParishUid { get; private set; }
        public string? ConnectionString { get; private set; }

        public CurrentParishService(CentralDbContext context)
        {
            _context = context;
        }

        public async Task<bool> SetParish(string parishUid)
        {
            var parish = await _context.Parishes
                .SingleOrDefaultAsync(p => p.UniqueId.ToString() == parishUid);

            if (parish != null)
            {
                ParishUid = parishUid;
                ConnectionString = parish.EncryptedConnectionString;

                return true;
            }
            else
            {
                throw new InvalidOperationException($"Parish with UID {parishUid} not found.");
            }
        }
    }

    /// <summary>
    /// Interfejs serwisu do ³adowania i przechowywania obecnie wybranej parafii.
    /// </summary>
    public interface ICurrentParishService
    {
        /// <summary>
        /// Publiczny unikalny identyfikator obecnie wybranej parafii.
        /// </summary>
        public string? ParishUid { get; }

        /// <summary>
        /// ConnectionString wybranej parafii w postaci jawnej.
        /// </summary>
        public string? ConnectionString { get; }

        /// <summary>
        /// Ustawia wybran¹ parafiê na podstawie jej publicznego unikalnego identyfikatora (UID).
        /// </summary>
        /// <param name="parishUid">Publiczny unikalny identyfikator parafii (UID)</param>
        /// <returns>True jeœli parafia zosta³a znaleziona i ustawiona, false w przeciwnym wypadku</returns>
        Task<bool> SetParish(string parishUid);
    }
}