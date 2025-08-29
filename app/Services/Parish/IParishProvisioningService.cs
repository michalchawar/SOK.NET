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
    /// Interfejs do tworzenia indywidualnych baz danych dla parafii
    /// oraz zarządania ich stanem (istnienie, migracje).
    /// </summary>
    public interface IParishProvisioningService
    {
        /// <summary>
        /// Tworzy nową parafię w systemie.
        /// Przygotowuje bazę danych dla niej (łącznie z użytkownikiem bazy i migracjami)
        /// i zapisuje connection string w modelu, a model w centralnej bazie.
        /// </summary>
        /// <param name="parishUid">Publiczny unikalny identyfikator parafii (UID)</param>
        /// <param name="parishName">Nazwa parafii</param>
        /// <returns>Obiekt ParishEntry, reprezentujący nowo utworzoną parafię</returns>
        /// <exception cref="InvalidOperationException"></exception>
        Task<ParishEntry> CreateParishAsync(string parishUid, string parishName);

        /// <summary>
        /// Sprawdza, czy wszystkie parafialne bazy istnieją i mają aktualne migracje.
        /// Jeśli nie, to je tworzy i/lub wykonuje migracje.
        /// </summary>
        /// <returns></returns>
        Task EnsureAllParishDatabasesReadyAsync();
    }
}
