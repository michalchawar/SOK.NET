using app.Models;
using app.Data;
using Microsoft.EntityFrameworkCore;
using app.Services;

namespace app.Services.Parish
{
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